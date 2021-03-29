using System;

namespace mongodb
{
	public class ParameterValue
	{
		public Guid Id { get; set; }
		public Guid ParameterId { get; set; }
		public DataSourceId DataSourceId { get; set; }
	}
}