﻿<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <connectionStrings>
    <clear />
    <add name="Default" connectionString="Server=(local);Persist Security Info=True;Integrated Security=true;"
         providerName="System.Data.SqlClient" />
  </connectionStrings>
  <appSettings>
    <add key="cmd" 
         value="declare @db varchar(max) = DB_NAME(); exec sp_helpdb @db;
select * from sys.tables order by create_date desc"
    />
  </appSettings>
</configuration>