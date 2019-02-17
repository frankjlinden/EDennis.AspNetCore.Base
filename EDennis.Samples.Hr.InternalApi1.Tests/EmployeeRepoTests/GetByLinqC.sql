﻿use hr;

declare @alpha varchar(30) = 'y'

declare 
	@expected varchar(max) = 
(
	select * from Employee
	where FirstName like '%' + @alpha + '%'
	for json path, include_null_values
);

exec _maintenance.SaveTestJson 'EDennis.Samples.Hr.InternalApi1', 'EmployeeRepo', 'GetByLinq','GetByLinq','C','Alpha', @alpha
exec _maintenance.SaveTestJson 'EDennis.Samples.Hr.InternalApi1', 'EmployeeRepo', 'GetByLinq','GetByLinq','C','Expected', @expected

--exec _maintenance.ResetIdentities
