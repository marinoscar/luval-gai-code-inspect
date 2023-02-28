       >>SOURCE FORMAT IS FIXED
      identification division.
       program-id. sqlscreen.

       data division.
       working-storage section.
       01 name-length          constant 20.
       01 value-length         constant 132.

      
      *><[
       01 database             pic x(8) value 'test.db' & x'00'.
      *><]
       01 db                   usage pointer.
       01 callback-proc        usage procedure-pointer.
       01 errstr               pic x(80).
       01 result               pic s9(9).

       01 query                pic x(255).
       01 zquery               pic x(256).

       01 main-record.
          03 key-field         pic 9(10).
          03 str-field         pic x(20).
          03 date-field        pic x(20).

       01 sql-table            external.
          03 sql-records       pic x(50) occurs 20 times.

       01 row-counter          usage binary-long external.
       01 row-max              usage binary-long.

       screen section.
       01 entry-screen.
          05 foreground-color 0 background-color 7 blank screen.
          05 foreground-color 0 background-color 7
             line 1 col 14 pic x(20) value "select * from trial;".
          05 foreground-color 0 background-color 7
             line 2 col 4 pic x(8) value "Key:".
          05 foreground-color 0 background-color 7
             line 2 col 14 pic x(10) using key-field.
          05 foreground-color 0 background-color 7
             line 3 col 4 pic x(8) value "String:".
          05 foreground-color 0 background-color 7
             line 3 col 14 pic x(20) from str-field.
          05 foreground-color 0 background-color 7
             line 4 col 4 pic x(8) value "Date:".
          05 foreground-color 0 background-color 7
             line 4 col 14 pic x(20) from date-field.
          05 foreground-color 0 background-color 7
             line 6 col 4 pic x(17) value "Hit ENTER to page".

      
       procedure division.

      
       call "ocsqlite_init" using
               db
               database
               by reference errstr
               by value function length(errstr)
           returning result
       end-call
       if result not equal zero
           display "Result: " result end-display
       end-if
      
       set callback-proc to entry "callback"
      
    >>Dmove ".echo on" to query
    >>Dperform ocsql-exec

    >>Dmove ".help" to query
    >>Dperform ocsql-exec

    >>Dmove ".tables" to query
    >>Dperform ocsql-exec

    >>Dmove ".timer on" to query
    >>Dperform ocsql-exec

    >>Dmove ".mode tcl" to query
    >>Dperform ocsql-exec

    >>Dmove 0 to row-counter
    >>Dmove "select * from trial;" to query
    >>Dperform ocsql-exec

    >>Dmove ".mode html" to query
    >>Dperform ocsql-exec

    >>Dmove 'insert into trial values (null, "string", "2008-10-10");'
    >>D  to query
    >>Dperform ocsql-exec

    >>Dmove "select * from thisfails;" to query
    >>Dperform ocsql-exec

       move "drop table trial;" to query
       perform ocsql-exec

       move "create table trial (first integer primary key, " &
           "second char(20), third date);" to query
       perform ocsql-exec

    >>Dmove "pragma count_changes=1;"  to query
    >>Dperform ocsql-exec

    >>Dmove "pragma database_list;"  to query
    >>Dperform ocsql-exec

    >>Dmove ".schema trial" to query
    >>Dperform ocsql-exec

       move 'insert into trial (first, second, third) values ' &
           '(null, lower(hex(randomblob(20))), datetime()); ' &
           'insert into trial values (null, "something",' &
           ' julianday());' to query
       perform ocsql-exec

    >>Dmove "select * from trial;" to query
    >>Dperform ocsql-exec

    >>Dmove "pragma count_changes=0;"  to query
    >>Dperform ocsql-exec
      
       move 'insert into trial (first, second, third) values ' &
           '(null, lower(hex(randomblob(20))), datetime()); ' &
           'insert into trial values (null, "something",' &
           ' julianday());' to query
       perform ocsql-exec
      
       move ".mode column" to query
       perform ocsql-exec

       move ".width 10 20 20" to query
       perform ocsql-exec

       move 1 to row-counter
       move "select * from trial;" to query
       perform ocsql-exec
       display function trim(sql-table trailing) end-display

       subtract 1 from row-counter giving row-max end-subtract
       perform varying row-counter from row-max by -1
           until row-counter < 1
               move sql-records(row-counter) to main-record
               display "|" key-field "|" end-display
               display "|" str-field "|" end-display
               display "|" date-field "|" end-display
       end-perform
      
       perform varying row-counter from 1 by 1
           until row-counter > row-max
           move sql-records(row-counter) to main-record
           accept entry-screen end-accept
       end-perform
      
       goback.

      
       call "ocsqlite_close"
           using
               by value db
           returning result
       end-call

       move result to return-code
       goback.
      
       ocsql-exec.
       move spaces to zquery
       string
           function trim(query trailing) delimited by size
           x"00" delimited by size
           into zquery
       end-string
      
       call "ocsqlite"
           using by value db
               callback-proc
               by reference zquery
               by value function length(zquery)
               by reference errstr
               by value function length(errstr)
           returning result
       end-call
       if result not equal 0
           display "Err:    " errstr end-display
       end-if
       .

       end program sqlscreen.
      
       identification division.
       program-id. callback.

       data division.
       working-storage section.
       01 count-display        pic z9.
       01 index-display        pic z9.

       01 value-display        pic x(132).

       01 main-record.
          03 field-1           pic 9(10).
          03 field-2           pic x(20).
          03 field-3           pic x(20).
          03 filler            pic x(82).

       01 row-counter          usage binary-long external.

       01 sql-table            external.
          03 sql-records       pic x(50) occurs 20 times.

       linkage section.
       01 nada                 usage pointer.
       01 field-count          usage binary-long.
       01 row-data             pic x(132).
       01 row-length           usage binary-long.

      
       procedure division using
           nada field-count row-data row-length.

      
       move spaces to value-display
       string
           row-data delimited by low-value
           into value-display
       end-string
       inspect value-display replacing all x"0a" by space

      
       move value-display to main-record
       if row-counter > 0
           move main-record to sql-records(row-counter)
           add 1 to row-counter end-add
       end-if
      
    >>Ddisplay "["
    >>D    function trim(main-record trailing)
    >>D"]" end-display

       move 0 to return-code
       goback.

       end program callback.