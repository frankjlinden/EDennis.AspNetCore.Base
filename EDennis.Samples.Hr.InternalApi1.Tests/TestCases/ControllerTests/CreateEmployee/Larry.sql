﻿use hr;

declare @firstName varchar(30) = 'Larry'
declare @Input varchar(max) = 
( 
	select @firstName FirstName
	for json path, include_null_values, without_array_wrapper
);


begin transaction
insert into Employee(FirstName)
	values (@firstName);

declare @Expected varchar(max) = 
(
	select * from Employee
	for json path, include_null_values
);
rollback transaction
exec _maintenance.ResetIdentities

exec _maintenance.SaveTestJson 'EDennis.Samples.Hr.InternalApi1', 'EmployeeController', 'CreateEmployee','CreateAndGetAll',@firstName,'Input', @input
exec _maintenance.SaveTestJson 'EDennis.Samples.Hr.InternalApi1', 'EmployeeController', 'CreateEmployee','CreateAndGetAll',@firstName,'Expected', @expected

select * from _maintenance.TestJson
	where ProjectName = 'EDennis.Samples.Hr.InternalApi1'
		and ClassName = 'EmployeeController'
		and MethodName = 'CreateEmployee'
		and TestScenario = 'CreateAndGetAll'
		and TestCase = @firstName