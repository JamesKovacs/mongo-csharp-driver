using System;

namespace mongodb
{
	public class CustomDataSourceId : DataSourceId
	{
		public Guid Id { get; set; }
	}
}