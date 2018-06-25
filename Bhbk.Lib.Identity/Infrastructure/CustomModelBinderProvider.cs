using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Infrastructure
{
    public class CustomPagingModel
    {
        private int _pageSizeMax = 100;

        [Required]
        public string OrderBy { get; private set; }

        [Required]
        public int? PageNumber { get; private set; }

        [Required]
        public int? PageSize { get; private set; }

        public CustomPagingModel(string orderBy, int pageSize, int pageNumber)
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
    public class CustomPagingModelBinder : IModelBinder
    {
        private readonly IModelBinder _binder;

        public CustomPagingModelBinder() { }

        public CustomPagingModelBinder(IModelBinder binder)
        {
            if (binder == null)
                throw new ArgumentNullException(nameof(binder));

            _binder = binder;
        }

        public Task BindModelAsync(ModelBindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var orderByValue = "orderBy";
            var pageSizeValue = "pageSize";
            var pageNumberValue = "pageNumber";
            var errorMsg = "null";

            var orderBy = context.ValueProvider.GetValue(orderByValue);

            if (orderBy == ValueProviderResult.None)
                context.ModelState.AddModelError(orderByValue, errorMsg);

            var pageSize = context.ValueProvider.GetValue(pageSizeValue);

            if (pageSize == ValueProviderResult.None)
                context.ModelState.AddModelError(pageSizeValue, errorMsg);

            var pageNumber = context.ValueProvider.GetValue(pageNumberValue);

            if (pageNumber == ValueProviderResult.None)
                context.ModelState.AddModelError(pageNumberValue, errorMsg);

            if (context.ModelState.ErrorCount == 0)
                context.Result = ModelBindingResult.Success(
                    new CustomPagingModel(orderBy.FirstValue, Convert.ToInt32(pageSize.FirstValue), Convert.ToInt32(pageNumber.FirstValue)));

            else if (context.ModelState.ErrorCount > 0)
                context.Result = ModelBindingResult.Failed();

            return Task.CompletedTask;
        }
    }

    public class CustomPagingModelBinderProvider : IModelBinderProvider
    {
        private readonly IModelBinderProvider _binder;

        public CustomPagingModelBinderProvider() { }

        public CustomPagingModelBinderProvider(IModelBinderProvider binder)
        {
            if (binder == null)
                throw new ArgumentNullException(nameof(binder));

            _binder = binder;
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.Metadata.IsComplexType && context.Metadata.ModelType == typeof(CustomPagingModel))
                return new CustomPagingModelBinder();

            //if (context.Metadata.IsComplexType && context.Metadata.ModelType == typeof(CustomPagingModel))
            //    return new CustomPagingModelBinder(new ComplexTypeModelBinder(context.Metadata.Properties.ToDictionary(x => x, context.CreateBinder)));

            return null;
        }
    }

    public static class MvcOptionsExtensions
    {
        public static void UseCustomPagingModelBinder(this MvcOptions options)
        {
            var binder = options.ModelBinderProviders.FirstOrDefault(x => x.GetType() == typeof(ComplexTypeModelBinderProvider));

            if (binder == null)
                return;

            var index = options.ModelBinderProviders.IndexOf(binder);

            options.ModelBinderProviders.Insert(index, new CustomPagingModelBinderProvider());
        }
    }
}
