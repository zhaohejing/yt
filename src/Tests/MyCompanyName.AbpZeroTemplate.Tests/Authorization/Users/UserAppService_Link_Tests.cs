using System;
using System.Data.Entity;
using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.Domain.Uow;
using Abp.Runtime.Security;
using YT.Authorization.Users;
using YT.Authorization.Users.Dto;
using YT.MultiTenancy;
using Shouldly;
using Xunit;
using YT.Managers.MultiTenancy;
using YT.Managers.Users;

namespace YT.Tests.Authorization.Users
{
    public class UserAppService_Link_Tests : UserAppServiceTestBase
    {
        private readonly IUserLinkManager _userLinkManager;
        private readonly UserManager _userManager;
        private readonly TenantManager _tenantManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public UserAppService_Link_Tests()
        {
            _userLinkManager = Resolve<IUserLinkManager>();
            _userManager = Resolve<UserManager>();
            _tenantManager = Resolve<TenantManager>();
            _unitOfWorkManager = Resolve<IUnitOfWorkManager>();
        }

        [MultiTenantFact]
        public async Task Should_Link_User_To_Host_Admin()
        {
            LoginAsHostAdmin();
            await LinkUserAndTestAsync(string.Empty);
        }

        [MultiTenantFact]
        public async Task Should_Link_User_To_Default_Tenant_Admin()
        {
            LoginAsDefaultTenantAdmin();
            await LinkUserAndTestAsync(Tenant.DefaultTenantName);
        }

        [MultiTenantFact]
        public async Task Should_Link_User_To_Different_Tenant_User()
        {
            //Arrange
            LoginAsHostAdmin();
            var testTenantId = await CreateTestTenantAndTestUser();

            //Act
            LoginAsDefaultTenantAdmin();
         

            //Assert
            var defaultTenantAdmin = await UsingDbContextAsync(context => context.Users.FirstOrDefaultAsync(u => u.TenantId == AbpSession.TenantId && u.Id == AbpSession.UserId));
            var defaultTenantAccount = await _userLinkManager.GetUserAccountAsync(defaultTenantAdmin.ToUserIdentifier());

            var testUser = await UsingDbContextAsync(testTenantId, context => context.Users.FirstOrDefaultAsync(u => u.UserName == "test"));
            var testUserAccount = await _userLinkManager.GetUserAccountAsync(testUser.ToUserIdentifier());

            defaultTenantAccount.UserLinkId.ShouldBe(testUserAccount.UserLinkId);
            defaultTenantAccount.UserLinkId.ShouldBe(defaultTenantAccount.Id);
        }

        [MultiTenantFact]
        public async Task Should_Link_User_To_Already_Linked_User()
        {
            //Arrange
            LoginAsHostAdmin();
            var testTenantId = await CreateTestTenantAndTestUser();

            LoginAsDefaultTenantAdmin();
            await CreateTestUsersForAccountLinkingAsync();

            var linkToTestTenantUserInput = new LinkToUserInput
            {
                TenancyName = "Test",
                UsernameOrEmailAddress = "test",
                Password = "123qwe"
            };

            //Act
            //Link Default\admin -> Test\test

            LoginAsTenant(Tenant.DefaultTenantName, "jnash");
            //Link Default\jnash->Test\test

            //Assert
            var defaultTenantAdmin = await UsingDbContextAsync(context => context.Users.FirstOrDefaultAsync(u => u.TenantId == AbpSession.TenantId && u.UserName == User.AdminUserName));
            var defaultTenantAdminAccount = await _userLinkManager.GetUserAccountAsync(defaultTenantAdmin.ToUserIdentifier());

            var jnash = await UsingDbContextAsync(context => context.Users.FirstOrDefaultAsync(u => u.UserName == "jnash"));
            var jnashAccount = await _userLinkManager.GetUserAccountAsync(jnash.ToUserIdentifier());

            var testTenantUser = await UsingDbContextAsync(testTenantId, context => context.Users.FirstOrDefaultAsync(u => u.UserName == "test"));
            var testTenantUserAccount = await _userLinkManager.GetUserAccountAsync(testTenantUser.ToUserIdentifier());

            jnashAccount.UserLinkId.ShouldBe(jnashAccount.Id);
            defaultTenantAdminAccount.UserLinkId.ShouldBe(jnashAccount.Id);
            testTenantUserAccount.UserLinkId.ShouldBe(jnashAccount.Id);

            jnashAccount.UserLinkId.ShouldBe(defaultTenantAdminAccount.UserLinkId);
            jnashAccount.UserLinkId.ShouldBe(testTenantUserAccount.UserLinkId);
        }

        private async Task<int> CreateTestTenantAndTestUser()
        {
            var testTenant = new Tenant("Test", "test")
            {
                ConnectionString = SimpleStringCipher.Instance.Encrypt("Server=localhost; Database=AbpZeroTemplateTest_" + Guid.NewGuid().ToString("N") + "; Trusted_Connection=True;")
            };

            await _tenantManager.CreateAsync(testTenant);

            using (var uow = _unitOfWorkManager.Begin())
            {
                using (_unitOfWorkManager.Current.SetTenantId(testTenant.Id))
                {
                    var testUser = new User
                    {
                        EmailAddress = "test@test.com",
                        IsEmailConfirmed = true,
                        Name = "Test",
                        Surname = "User",
                        UserName = "test",
                        Password = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==", //123qwe
                        TenantId = testTenant.Id,
                    };

                    await _userManager.CreateAsync(testUser);

                    UsingDbContext(null, context =>
                     {
                         context.UserAccounts.Add(new UserAccount
                         {
                             TenantId = testUser.TenantId,
                             UserId = testUser.Id,
                             UserName = testUser.UserName,
                             EmailAddress = testUser.EmailAddress
                         });
                     });
                }

                await uow.CompleteAsync();
                return testTenant.Id;
            }
        }

        private async Task LinkUserAndTestAsync(string tenancyName)
        {
            //Arrange
            await CreateTestUsersForAccountLinkingAsync();

            //Act
          

            //Assert
            var linkedUser = await UsingDbContextAsync(context => context.Users.FirstOrDefaultAsync(u => u.UserName == "jnash"));
            var linkedUserAccount = await _userLinkManager.GetUserAccountAsync(linkedUser.ToUserIdentifier());

            var currentUser = await UsingDbContextAsync(context => context.Users.FirstOrDefaultAsync(u => u.Id == AbpSession.UserId));
            var currentUserAccount = await _userLinkManager.GetUserAccountAsync(currentUser.ToUserIdentifier());

            linkedUserAccount.UserLinkId.HasValue.ShouldBe(true);
            currentUserAccount.UserLinkId.HasValue.ShouldBe(true);

            linkedUserAccount.UserLinkId.Value.ShouldBe(currentUserAccount.UserLinkId.Value);
        }

        private async Task CreateTestUsersForAccountLinkingAsync()
        {
            await _userManager.CreateAsync(CreateUserEntity("jnash", "John", "Nash", "jnsh2000@testdomain.com"));
            await _userManager.CreateAsync(CreateUserEntity("adams_d", "Douglas", "Adams", "adams_d@gmail.com"));
            await _userManager.CreateAsync(CreateUserEntity("artdent", "Arthur", "Dent", "ArthurDent@yahoo.com"));
        }
    }
}
