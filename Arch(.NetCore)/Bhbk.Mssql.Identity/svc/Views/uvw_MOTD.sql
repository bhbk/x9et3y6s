
CREATE VIEW [svc].[uvw_MOTD]
AS
SELECT        Id, Author, Quote, TssId, TssTitle, TssCategory, TssLength, TssDate, TssTags, TssBackground
FROM            dbo.tbl_MOTD