# frozen_string_literal: true

module ActiveRecord
  module Querying
    QUERYING_METHODS = [
      :find, :find_by, :find_by!, :take, :take!, :sole, :find_sole_by, :first, :first!, :last, :last!,
      :second, :second!, :third, :third!, :fourth, :fourth!, :fifth, :fifth!,
      :forty_two, :forty_two!, :third_to_last, :third_to_last!, :second_to_last, :second_to_last!,
      :exists?, :any?, :many?, :none?, :one?,
      :first_or_create, :first_or_create!, :first_or_initialize,
      :find_or_create_by, :find_or_create_by!, :find_or_initialize_by,
      :create_or_find_by, :create_or_find_by!,
      :destroy_all, :delete_all, :update_all, :touch_all, :destroy_by, :delete_by,
      :find_each, :find_in_batches, :in_batches,
      :select, :reselect, :order, :regroup, :in_order_of, :reorder, :group, :limit, :offset, :joins, :left_joins, :left_outer_joins,
      :where, :rewhere, :invert_where, :preload, :extract_associated, :eager_load, :includes, :from, :lock, :readonly,
      :and, :or, :annotate, :optimizer_hints, :extending,
      :having, :create_with, :distinct, :references, :none, :unscope, :merge, :except, :only,
      :count, :average, :minimum, :maximum, :sum, :calculate,
      :pluck, :pick, :ids, :async_ids, :strict_loading, :excluding, :without, :with,
      :async_count, :async_average, :async_minimum, :async_maximum, :async_sum, :async_pluck, :async_pick,
    ].freeze # :nodoc:
    delegate(*QUERYING_METHODS, to: :all)

    
    def find_by_sql(sql, binds = [], preparable: nil, &block)
      _load_from_sql(_query_by_sql(sql, binds, preparable: preparable), &block)
    end

    
    def async_find_by_sql(sql, binds = [], preparable: nil, &block)
      _query_by_sql(sql, binds, preparable: preparable, async: true).then do |result|
        _load_from_sql(result, &block)
      end
    end

    def _query_by_sql(sql, binds = [], preparable: nil, async: false) # :nodoc:
      connection.select_all(sanitize_sql(sql), "#{name} Load", binds, preparable: preparable, async: async)
    end

    def _load_from_sql(result_set, &block) # :nodoc:
      column_types = result_set.column_types

      unless column_types.empty?
        column_types = column_types.reject { |k, _| attribute_types.key?(k) }
      end

      message_bus = ActiveSupport::Notifications.instrumenter

      payload = {
        record_count: result_set.length,
        class_name: name
      }
      message_bus.instrument("instantiation.active_record", payload) do
        if result_set.includes_column?(inheritance_column)
          result_set.map { |record| instantiate(record, column_types, &block) }
        else
          result_set.map { |record| instantiate_instance_of(self, record, column_types, &block) }
        end
      end
    end

    
    def count_by_sql(sql)
      connection.select_value(sanitize_sql(sql), "#{name} Count").to_i
    end

    def async_count_by_sql(sql)
      connection.select_value(sanitize_sql(sql), "#{name} Count", async: true).then(&:to_i)
    end
  end
end