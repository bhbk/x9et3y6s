



CREATE VIEW [svc].[uvw_MOTDs]
AS
SELECT        Id, Author, Quote, TssId, TssTitle, TssCategory, TssLength, TssDate, TssTags, TssBackground
FROM            dbo.tbl_MOTDs