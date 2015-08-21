set nocount on

use AntHill

declare @ant table (antId uniqueidentifier)
declare @cell table (cellId uniqueidentifier)

declare @i int
set @i = 100
while @i > 0
begin
	set @i = @i - 1
	begin tran
	declare @j int
	set @j = 3907
	while @j > 0
	begin
		set @j = @j - 1

		declare @type tinyint
		set @type = 255
		while (1=1)
		begin
			insert Ants (type)
			output inserted.Id into @ant
			values (@type)

			insert Cells (area)
			output inserted.Id into @cell
			values (0)

			insert Links
			select antId, cellId
			from @ant cross join @cell

			delete @ant
			delete @cell
			
			if @type = 0
				break
			else
				set @type = @type-1
		end
	end

	commit tran
	select count(*) from links
end