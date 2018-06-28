using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.Lib.Identity.Infrastructure
{
    public class PagingModel
    {
        private int _pageSizeMax = 100;

        [Required]
        public string OrderBy { get; private set; }

        [Required]
        public int? PageNumber { get; private set; }

        [Required]
        public int? PageSize { get; private set; }

        public PagingModel(string orderBy, int pageSize, int pageNumber)
        {
            PageNumber = pageNumber;

            if (PageSize > _pageSizeMax)
                PageSize = _pageSizeMax;
            else
                PageSize = pageSize;

            OrderBy = orderBy;
        }
    }

    //https://www.dotnetcurry.com/aspnet-mvc/1368/aspnet-core-mvc-custom-model-binding
    public class PagingModelBinder : IModelBinder
    {
        private readonly IModelBinder _binder;

        public PagingModelBinder() { }

        public PagingModelBinder(IModelBinder binder)
        {
            if (binder == null)
                throw new ArgumentNullException(nameof(binder));

            _binder = binder;
        }

        public Task BindModelAsync(ModelBindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var errorMsg = "null";

            var orderBy = context.ValueProvider.GetValue(BaseLib.Statics.GetOrderBy);

            if (orderBy == ValueProviderResult.None)
                context.ModelState.AddModelError(BaseLib.Statics.GetOrderBy, errorMsg);

            var pageSize = context.ValueProvider.GetValue(BaseLib.Statics.GetPageSize);

            if (pageSize == ValueProviderResult.None)
                context.ModelState.AddModelError(BaseLib.Statics.GetPageSize, errorMsg);

            var pageNumber = context.ValueProvider.GetValue(BaseLib.Statics.GetPageNumber);

            if (pageNumber == ValueProviderResult.None)
                context.ModelState.AddModelError(BaseLib.Statics.GetPageNumber, errorMsg);

            if (context.ModelState.ErrorCount == 0)
                context.Result = ModelBindingResult.Success(
                    new PagingModel(orderBy.FirstValue, Convert.ToInt32(pageSize.FirstValue), Convert.ToInt32(pageNumber.FirstValue)));

            else if (context.ModelState.ErrorCount > 0)
                context.Result = ModelBindingResult.Failed();

            return Task.CompletedTask;
        }
    }

    public class ModelBinderProvider : IModelBinderProvider
    {
        private readonly IModelBinderProvider _binder;

        public ModelBinderProvider() { }

        public ModelBinderProvider(IModelBinderProvider binder)
        {
            if (binder == null)
                throw new ArgumentNullException(nameof(binder));

            _binder = binder;
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.Metadata.IsComplexType && context.Metadata.ModelType == typeof(PagingModel))
                return new PagingModelBinder();

            //if (context.Metadata.IsComplexType && context.Metadata.ModelType == typeof(CustomPagingModel))
            //    return new CustomPagingModelBinder(new ComplexTypeModelBinder(context.Metadata.Properties.ToDictionary(x => x, context.CreateBinder)));

            return null;
        }
    }

    public static class MvcOptionsExtensions
    {
        public static void UseMyModelBinders(this MvcOptions options)
        {
            var binder = options.ModelBinderProviders.FirstOrDefault(x => x.GetType() == typeof(ComplexTypeModelBinderProvider));

            if (binder == null)
                return;

            var index = options.ModelBinderProviders.IndexOf(binder);

            options.ModelBinderProviders.Insert(index, new ModelBinderProvider());
        }
    }
}
