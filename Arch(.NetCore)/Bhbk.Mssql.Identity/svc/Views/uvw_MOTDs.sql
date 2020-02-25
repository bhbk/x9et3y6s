
CREATE VIEW [svc].[uvw_MOTDs]
AS
SELECT        Id, Title, Author, Quote, Category, Date, Tags, Length, Background
FROM            dbo.tbl_MOTDs