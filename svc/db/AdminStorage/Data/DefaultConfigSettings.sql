IF NOT EXISTS(SELECT [KEY] FROM [AdminStore].[ConfigSettings] WHERE [Key]=N'DaysToKeepInLogs')
BEGIN
	INSERT INTO [AdminStore].[ConfigSettings] ([Key], [Value], [Group], [IsRestricted])
		 VALUES (N'DaysToKeepInLogs', N'7', N'Maintenance', 0)
END