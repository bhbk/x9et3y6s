using AutoMapper;
using Bhbk.Lib.Identity.Domain.Profiles;

namespace Bhbk.Lib.Identity.Domain.Builders
{
    /*
     * base class for entity builders using CRTP for fluent chaining
     */
    public abstract class EntityBuilderBase<TBuilder, TEntity>
        where TBuilder : EntityBuilderBase<TBuilder, TEntity>
        where TEntity : class
    {
        protected static readonly IMapper Map;

        static EntityBuilderBase()
        {
            Map = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile>()).CreateMapper();
        }

        protected TBuilder Self => (TBuilder)this;

        public abstract TEntity Build();
    }
}
