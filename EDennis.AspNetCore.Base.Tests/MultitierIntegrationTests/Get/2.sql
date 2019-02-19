﻿use colordb;
declare @Id int = 2
declare @Input varchar(max) = @Id;
declare @Expected varchar(max) = (
	select Id, Name
		from Colors
		where Id = @Id
		for json path, without_array_wrapper
);
exec _maintenance.SaveTestJson 'EDennis.Samples.Colors.InternalApi','ColorController','Get','HttpClientExtensions',@Id,'Input',@Input
exec _maintenance.SaveTestJson 'EDennis.Samples.Colors.InternalApi','ColorController','Get','HttpClientExtensions',@Id,'Expected',@Expected
exec  _maintenance.GetTestJson 'EDennis.Samples.Colors.InternalApi','ColorController','Get','HttpClientExtensions',@Id