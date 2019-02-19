﻿use hr;
declare @TestCase varchar(1) = 'C'
declare @PageNumber int = 1
declare @PageSize int = 1
declare @Alpha varchar(30) = 'n'

declare @Expected varchar(max) = 
(
	select * from Employee
		where FirstName like '%' + @alpha + '%'
		order by id
		offset @PageSize * (@PageNumber - 1) rows
		fetch next @PageSize rows only 
		for json path, include_null_values
);


exec _maintenance.SaveTestJson 'EDennis.Samples.Hr.InternalApi1', 'EmployeeRepo', 'Query','PagedList',@TestCase,'Alpha', @Alpha
exec _maintenance.SaveTestJson 'EDennis.Samples.Hr.InternalApi1', 'EmployeeRepo', 'Query','PagedList',@TestCase,'PageNumber', @PageNumber
exec _maintenance.SaveTestJson 'EDennis.Samples.Hr.InternalApi1', 'EmployeeRepo', 'Query','PagedList',@TestCase,'PageSize', @PageSize
exec _maintenance.SaveTestJson 'EDennis.Samples.Hr.InternalApi1', 'EmployeeRepo', 'Query','PagedList',@TestCase,'Expected', @Expected

exec  _maintenance.GetTestJson 'EDennis.Samples.Hr.InternalApi1', 'EmployeeRepo', 'Query','PagedList',@TestCase

