using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Tests.Serialization.TestModels;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class GeneratedGenericTest : BaseSerialization
    {
        [Fact]
        public async Task GenericTest()
        {
            SerializersMap.RegisterSerializers(KeyValuePair.Create(typeof(GenericModel<long>), new MongoDB.Client.Bson.Serialization.Generated.MongoDBClientTestsSerializationTestModelsGenericModelTSerializerGenerated<long>() as IBsonSerializer));
            SerializersMap.RegisterSerializers(KeyValuePair.Create(typeof(GenericModel<NonGenericModel>), new MongoDB.Client.Bson.Serialization.Generated.MongoDBClientTestsSerializationTestModelsGenericModelTSerializerGenerated<NonGenericModel>() as IBsonSerializer));
            var simpleModel = new GenericModel<long>()
            {
                GenericValue = long.MaxValue,
                GenericList = new System.Collections.Generic.List<long>() { 1, 2, 3, 4, 5},
            };

            SerializersMap.TryGetSerializer<GenericModel<long>>(out var simpleserializer);
            var result = await RoundTripAsync<GenericModel<long>>(simpleModel, simpleserializer);

            var nongeneric = new NonGenericModel()
            {
                A = 24,
                B = 24,
                C = 24,
            };
            var docgeneric = new GenericModel<NonGenericModel>()
            {
                GenericValue = new NonGenericModel
                {
                    A = 42,
                    B = 42,
                    C = 42,
                },
                GenericList = new System.Collections.Generic.List<NonGenericModel>() { nongeneric, null, nongeneric, nongeneric },
            };
            SerializersMap.TryGetSerializer<GenericModel<NonGenericModel>> (out var docserializer);
            var docresult = await RoundTripAsync<GenericModel<NonGenericModel>>(docgeneric, docserializer);
        }
        [Fact]
        public async Task GenericTest2()
        {
            SerializersMap.RegisterSerializers(KeyValuePair.Create(
                typeof(BaseAdminRep<AdminIdentifier, AdminDtoImpl, AdminDtoArgs, AdminDtoArgsImpl, AdminDtoUpdateArgs, AdminDtoUpdateArgsImpl>),
                new MongoDB.Client.Bson.Serialization.Generated.MongoDBClientTestsSerializationTestModelsBaseAdminRepTATADTAATADATAUATAUDSerializerGenerated<AdminIdentifier, AdminDtoImpl, AdminDtoArgs, AdminDtoArgsImpl, AdminDtoUpdateArgs, AdminDtoUpdateArgsImpl>() as IBsonSerializer));
            var admin = new AdminIdentifier();
            admin.Name = "Admin";
            var admindto = new AdminDtoImpl();
            admindto.Name = "Admin";
            var adminargs = new AdminDtoArgs();
            adminargs.arg1 = 42;
            adminargs.arg2 = 42;
            adminargs.arg3 = 42;
            var adminargsimpl = new AdminDtoArgsImpl();
            adminargsimpl.arg1 = 42;
            adminargsimpl.arg2 = 42;
            adminargsimpl.arg3 = 42;        
            var adminupdateargs = new AdminDtoUpdateArgs();
            adminupdateargs.arg1 = 42;
            adminupdateargs.arg2 = 42;
            adminupdateargs.arg3 = 42;
            var adminupdateargsimpl = new AdminDtoUpdateArgsImpl();
            adminupdateargsimpl.arg1 = 42;
            adminupdateargsimpl.arg2 = 42;
            adminupdateargsimpl.arg3 = 42;
            var rep = new BaseAdminRep<AdminIdentifier, AdminDtoImpl, AdminDtoArgs, AdminDtoArgsImpl, AdminDtoUpdateArgs, AdminDtoUpdateArgsImpl>();
            rep.Admin = admin;
            rep.AdminArgs = adminargs;
            rep.AdminArgsUpdateDto = adminupdateargsimpl;
            rep.AdminDto = admindto;
            rep.AdminDtoArgs = adminargsimpl;

            SerializersMap.TryGetSerializer<BaseAdminRep<AdminIdentifier, AdminDtoImpl, AdminDtoArgs, AdminDtoArgsImpl, AdminDtoUpdateArgs, AdminDtoUpdateArgsImpl>>(out var docserializer);
            var docresult = await RoundTripAsync<BaseAdminRep<AdminIdentifier, AdminDtoImpl, AdminDtoArgs, AdminDtoArgsImpl, AdminDtoUpdateArgs, AdminDtoUpdateArgsImpl>>(rep, docserializer);

        }
    }
}