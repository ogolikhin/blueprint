IF NOT EXISTS(SELECT [KEY] FROM [dbo].[ConfigSettings] WHERE [Key]=N'DaysToKeepInLogs')
BEGIN
	INSERT INTO [dbo].[ConfigSettings] ([Key], [Value], [Group], [IsRestricted])
		 VALUES (N'DaysToKeepInLogs', N'7', N'Maintenance', 0)
END