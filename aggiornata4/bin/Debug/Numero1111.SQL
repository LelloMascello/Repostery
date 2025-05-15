DECLARE @NewIdQueries int
 IF NOT EXISTS (
SELECT 1  FROM [QueriesDb].[dbo].[Queries] WHERE [Name]='Numero1111'
) BEGIN
 	SET @NewIdQueries = (SELECT ISNULL(MAX(Id_Queries), 0) + 1 FROM [QueriesDb].[dbo].[Queries])
 INSERT INTO [QueriesDb].[dbo].Queries (Id_Queries, Name, Description, StoredProcedure, UserMandatory, [Order], Active, InternalQuery, IsExternalSupplierVisible, IsCustomReport, IsClientSidePivotQuery, UseExternalApi) VALUES (@NewIdQueries, 'Numero1111', 'Numero attività programmate per Commessa', 'SPS_ReportDynamic_MtActivitiesByContractSummary', 'True', '1', 'True', 'True', 'True', 'False', 'False', 'False');
 END
INSERT INTO [QueriesDb].[dbo].[Queries_CrossModules] (id_Cross_QueriesModules, id_Queries, Id_Module, [Order]) VALUES ((SELECT ISNULL(MAX(id_Cross_QueriesModules), 0) + 1 FROM [QueriesDb].[dbo].[Queries_CrossModules]), @NewIdQueries, '5', '4');

IF NOT EXISTS
(SELECT 1 FROM [QueriesDb].[dbo].[Queries_Parameter] WHERE [Description]='tr') 
BEGIN
INSERT INTO [QueriesDb].[dbo].[Queries_Parameter] (Id_Queries_Parameter, Id_Queries, Name, Description, Id_Queries_Parameter_Type, [Order], Id_Queries_Parameter_Relation, Active, Mandatory) VALUES ((SELECT ISNULL(MAX(Id_Queries_Parameter), 0) + 1 FROM [QueriesDb].[dbo].[Queries_Parameter]), @NewIdQueries, 'tr', 'tr', '3', '44', '4', 'True', 'True');
END

IF NOT EXISTS
(SELECT 1 FROM [QueriesDb].[dbo].[Queries_Parameter] WHERE [Description]='tr') 
BEGIN
INSERT INTO [QueriesDb].[dbo].[Queries_Parameter] (Id_Queries_Parameter, Id_Queries, Name, Description, Id_Queries_Parameter_Type, [Order], Id_Queries_Parameter_Relation, Active, Mandatory) VALUES ((SELECT ISNULL(MAX(Id_Queries_Parameter), 0) + 1 FROM [QueriesDb].[dbo].[Queries_Parameter]), @NewIdQueries, 'tr', 'tr', '3', '44', '4', 'True', 'True');
END

