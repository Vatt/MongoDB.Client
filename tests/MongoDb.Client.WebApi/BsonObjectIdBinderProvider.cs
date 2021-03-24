using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using MongoDB.Client.Bson.Document;

namespace MongoDb.Client.WebApi
{
    public class BsonObjectIdBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(BsonObjectId))
            {
                return new BinderTypeModelBinder(typeof(BsonObjectIdBinder));
            }

            return null;
        }
    }
}
