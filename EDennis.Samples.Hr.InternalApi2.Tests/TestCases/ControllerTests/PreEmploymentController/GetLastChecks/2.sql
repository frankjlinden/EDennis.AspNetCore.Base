﻿use AgencyOnlineCheck;

declare @EmployeeId int = 2
declare @AgencyOnlineInput varchar(max) = 
( 
	select
	@EmployeeId EmployeeId,
	'Fail' Status,
	'2018-12-02' DateCompleted
		for json path, include_null_values, without_array_wrapper
);
declare @FederalBackgroundInput varchar(max) = 
( 
	select
	@EmployeeId EmployeeId,
	'Pass' Status,
	'2019-01-02' DateCompleted
		for json path, include_null_values, without_array_wrapper
);

begin transaction
insert into AgencyOnlineCheck..AgencyOnlineCheck(EmployeeId, Status, DateCompleted)
	select
	@EmployeeId EmployeeId,
	'Fail' Status,
	'2018-12-02' DateCompleted
insert into FederalBackgroundCheck..FederalBackgroundCheck(EmployeeId, Status, DateCompleted)
	select
	@EmployeeId EmployeeId,
	'Pass' Status,
	'2019-01-02' DateCompleted

declare @Expected varchar(max) = (
	select 
		a.DateCompleted as [AgencyInvestigatorCheck.DateCompleted],
		a.Status as [AgencyInvestigatorCheck.Status],
		b.DateCompleted as [AgencyOnlineCheck.DateCompleted],
		b.Status as [AgencyOnlineCheck.Status],
		c.DateCompleted as [FederalBackgroundCheck.DateCompleted],
		c.Status as [FederalBackgroundCheck.Status],
		d.DateCompleted as [StateBackgroundCheck.DateCompleted],
		d.Status as [StateBackgroundCheck.Status]
		from 
		(select @EmployeeId EmployeeId) emps
		cross join
		(select top 1 DateCompleted, Status 
			from AgencyInvestigatorCheck..AgencyInvestigatorCheck 
			where EmployeeId = @EmployeeId
			order by DateCompleted desc) a
		cross join 
		(select top 1 DateCompleted, Status 
			from AgencyOnlineCheck..AgencyOnlineCheck
			where EmployeeId = @EmployeeId
			order by DateCompleted desc) b
		cross join 
		(select top 1 DateCompleted, Status 
			from FederalBackgroundCheck..FederalBackgroundCheck 
			where EmployeeId = @EmployeeId
			order by DateCompleted desc) c
		cross join 
		(select top 1 DateCompleted, Status 
			from StateBackgroundCheck..StateBackgroundCheck 
			where EmployeeId = @EmployeeId
			order by DateCompleted desc) d
		for json path);

rollback transaction
exec _maintenance.ResetIdentities

exec _maintenance.SaveTestJson 'EDennis.Samples.Hr.InternalApi2', 'PreEmploymentController', 'GetLastCheck', 'PostAndGet', @EmployeeId, 'AgencyOnlineInput', @AgencyOnlineInput
exec _maintenance.SaveTestJson 'EDennis.Samples.Hr.InternalApi2', 'PreEmploymentController', 'GetLastCheck', 'PostAndGet', @EmployeeId, 'FederalBackgroundInput', @FederalBackgroundInput
exec _maintenance.SaveTestJson 'EDennis.Samples.Hr.InternalApi2', 'PreEmploymentController', 'GetLastCheck', 'PostAndGet', @EmployeeId, 'EmployeeId', @EmployeeId
exec _maintenance.SaveTestJson 'EDennis.Samples.Hr.InternalApi2', 'PreEmploymentController', 'GetLastCheck', 'PostAndGet', @EmployeeId, 'Expected', @Expected

exec  _maintenance.GetTestJson 'EDennis.Samples.Hr.InternalApi2', 'PreEmploymentController', 'GetLastCheck', 'PostAndGet', @EmployeeId
		