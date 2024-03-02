CREATE TABLE [dbo].[observations] (
    [taxon_id]            NVARCHAR (MAX) NULL,
    [observation_uuid]    NVARCHAR (MAX) NULL,
    [observer_id]         NVARCHAR (MAX) NULL,
    [latitude]            NVARCHAR (MAX) NULL,
    [longitude]           NVARCHAR (MAX) NULL,
    [positional_accuracy] NVARCHAR (MAX) NULL,
    [quality_grade]       NVARCHAR (MAX) NULL,
    [observed_on]         NVARCHAR (MAX) NULL
);

