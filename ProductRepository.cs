using IDP.Authorizer;
using IDP.Common;
using IDP.DBX;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs.Views;
using  BackOffice.Authorizer.Management.Persistence.Api;
using BackOffice.Authorizer.Management.Persistence.Entities;
using BackOffice.Authorizer.Management.Persistence.Factory;
using RabbitMQ.Client.Framing.Impl;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class ProductRepository : IProductRepository
    {
        private readonly ICampaignConfigurationFactory campaignConfigurationFactory;
        private readonly IDataContext dataContext;
        private readonly ICityRepository cityRepository;

        public ProductRepository(
            IDataContext dataContext,
            ICampaignConfigurationFactory campaignConfigurationFactory,
            ICityRepository cityRepository)
        {
            this.campaignConfigurationFactory = campaignConfigurationFactory;
            this.dataContext = dataContext;
            this.cityRepository = cityRepository;
        }

        public async Task<short> GetProductFullBox(ProductSearchParameters parameters)
        {
            short fullbox = 0;
            var productDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
            using(var connectionContext = productDataContext.AcquireConnection())
            {
                var sql = @"SELECT
                            CASE [Prd_FullBox] WHEN 0 THEN 1
						    ELSE [Prd_FullBox]
						    END 
						    FROM [TrnCentre_ADV_V7].[dbo].[Prd_MsFile]
						    WHERE Acquirer_Code = @AcquirerCode
						    AND Rec_St = 'A'
						    AND Prd_StdCode =@EAN";

                var result = await connectionContext.QueryAsync<short>(sql, new
                {
                    AcquirerCode = parameters.AcquirerCode,
                    EAN = parameters.EAN
                });

                if (result != null && result.Any())
                {
                    fullbox = result.First();
                }
            }
           
            
            return fullbox;
        }

        public async Task<ProductItem[]> GetProductsByCompany(ProductSearchParameters parameters)
        {
            var products = new List<ProductItem>();
            var productDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
            using(var connectionContext = productDataContext.AcquireConnection())
            {
                var sql = @"select distinct prm.Prd_StdCode [EAN],
			                            cast(prm.Prd_StdCode as VARCHAR(15)) +  ' - '+ RTRIM(prm.Prd_Name)+ SPACE(1)
			                            + RTRIM(prm.Prd_Dose)
			                            + RTRIM(prm.Prd_PutUp)
			                            + RTRIM(prm.Prd_Pharma) 
			                            + SPACE(1)
			                            + 'Apresentação' [Description]
			                            ,prm.Prd_Brand [BrandId]
			                            ,RTRIM(mt.Table_Info1) [BrandDescription]
			                            ,pmf.Prd_AllowCRM [AllowCRM]
	                            from [TRNCENTRE_ADV_V7].[dbo].Prd_MsFile prm INNER JOIN
	                            [TRNCENTRE_ADV_V7].[dbo].Str_Multi_Table mt
	                            on prm.Prd_Brand = mt.Table_KeyCode INNER JOIN
	                            [TRNCENTRE_ADV_V7].[dbo].Prd_MsFile PMF
	                            on prm.Prd_StdCode = PMF.Prd_StdCode

	                            WHERE
		                            prm.Acquirer_Code = @AcquirerCode AND
		                            mt.Acquirer_Code = @AcquirerCode AND
		                            mt.Table_KeyType = 'Prd_Brand' AND
		                            mt.Rec_St = 'A' AND
		                            ((pmf.Prd_AllowCRM NOT IN ('N') AND @AllowCRM <> 'N') OR
		                             (pmf.Prd_AllowCRM IN ('N','O') AND @AllowCRM = 'N') ) ";

                var result = await connectionContext.QueryAsync<dynamic>(sql, new { AcquirerCode = parameters.AcquirerCode,
                AllowCRM = parameters.AllowCRM == true ? 'S' : 'N'
                });

                if(result != null && result.Any())
                {
                    products = result.Select(p => new ProductItem() {
                        AllowCRM = p.AllowCRM.ToString().Equals("N")? false:true,
                        EAN = p.EAN.ToString(),
                        Description = p.Description.ToString(),
                        Brand = new BrandViewDTO() { Id = Convert.ToInt32(p.BrandId.ToString()), Name = p.BrandDescription.ToString(),
                         }
                    }).ToList();
                }
            }
          
            return products.ToArray();
        }

        public async Task SaveProducts(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog)
        {
            
                    string sql = @"IF NOT EXISTS(SELECT Rules_Code FROM Rules_Prd WHERE Prd_StdCode = @Prd_StdCode AND
	                                    [State] = @State AND City_Code = @City_Code AND
	                                    Rules_Code = @Rules_Code AND Rules_BeginDate = @Rules_BeginDate)
	                                    BEGIN
		                                    INSERT INTO [dbo].[Rules_Prd]
                                                                            ([Rules_Code]
                                                                            ,[Rules_BeginDate]
                                                                            ,[Prd_StdCode]
                                                                            ,[State]
                                                                            ,[City_Code]
                                                                            ,[Prd_QtyMax]
                                                                            ,[Prc_Desc]
                                                                            ,[Mileage]
                                                                            ,[MultiPrd_Id]
                                                                            ,[Prd_CurrentBillQty]
                                                                            ,[Prc_CurrentBillDesc]
                                                                            ,[Trn_ProfessType]
                                                                            ,[Prc_PercCo]
                                                                            ,[Prc_PercIssuer]
                                                                            ,[Prc_AttendInfo]
                                                                            ,[Rec_St]
                                                                            ,[Rec_StInfo]
                                                                            ,[Rec_InsTime]
                                                                            ,[Rec_InsUser]
                                                                            ,[Rec_UpdTime]
                                                                            ,[Rec_UpdUser]
                                                                            ,[Rules_ComboId])
                                                                        VALUES
                                                                            (@Rules_Code
                                                                            ,@Rules_BeginDate
                                                                            ,@Prd_StdCode
                                                                            ,@State
                                                                            ,@City_Code
                                                                            ,@Prd_QtyMax
                                                                            ,@Prc_Desc
                                                                            ,@Mileage
                                                                            ,@MultiPrd_Id
                                                                            ,@Prd_CurrentBillQty
                                                                            ,@Prc_CurrentBillDesc
                                                                            ,@Trn_ProfessType
                                                                            ,@Prc_PercCo
                                                                            ,@Prc_PercIssuer
                                                                            ,@Prc_AttendInfo
                                                                            ,@Rec_St
                                                                            ,@Rec_StInfo
                                                                            ,@Rec_InsTime
                                                                            ,@Rec_InsUser
                                                                            ,@Rec_UpdTime
                                                                            ,@Rec_UpdUser
                                                                            ,@Rules_ComboId
		                                                                    )
	                                    END
	                                    ELSE
	                                    BEGIN 
		                                    UPDATE [dbo].[Rules_Prd]
		                                        SET 
			                                        [Prd_QtyMax] = @Prd_QtyMax
			                                        ,[Prc_Desc] = @Prc_Desc
			                                        ,[MultiPrd_Id] = @MultiPrd_Id
			                                        ,[Rec_UpdTime] = GETDATE()
			                                        ,[Rec_UpdUser] = @Rec_UpdUser
			                                        ,[Rules_ComboId] = @Rules_ComboId
			                                        ,[Rec_St] = 'A'
		                                        WHERE Rules_Code = @Rules_Code AND
			                                        Rules_BeginDate = @Rules_BeginDate AND
                                                    Prd_StdCode = @Prd_StdCode AND
	                                                [State] = @State AND City_Code = @City_Code
	                                    END
	                                    SELECT @@ROWCOUNT";

            using (var connectionContext = dataContext.AcquireConnection())
            {
                var sqlUpateProduct = @"UPDATE Rules_Prd SET Rec_St = 'D' WHERE Rules_Code = @Rules_Code AND Rules_BeginDate = @Rules_BeginDate";
                var sqlDeleteProduct = @"DELETE FROM Rules_Prd WHERE Rules_Code = @Rules_Code AND Rules_BeginDate = @Rules_BeginDate";
                await connectionContext.QueryAsync<int>(environmentType.Equals(CampaignEnvironmentType.Production)?sqlDeleteProduct:sqlUpateProduct, new
                {
                    Rules_BeginDate = campaign.BeginDate,
                    Rules_Code = campaign.Id
                });

                if (campaign.Products != null && campaign.Products.Count() > 0)
                {
                    foreach (var product in campaign.Products)
                    {
                        var fullbox = await this.GetProductFullBox(new ProductSearchParameters(campaign.AcquirerCode,product.EAN, false) );
                        foreach (var state in product.States)
                        {
                            foreach (var city in state.Cities)
                            {


                                var maxQuantity = (short)((product.MaxQuantity * campaign.LimitMode) / fullbox);

                                Dictionary<string, object> messageParameters = new Dictionary<string, object>();
                                messageParameters.Add("FUN", "IRC_InserirProdutos");
                                messageParameters.Add("RLC", campaign.Id);
                                messageParameters.Add("RBD", campaign.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));
                                messageParameters.Add("CTC", city.Id);
                                messageParameters.Add("MPC", product.DiscountType);
                                messageParameters.Add("PRD", product.Discount);
                                messageParameters.Add("PSC", product.EAN);
                                messageParameters.Add("PMQ", maxQuantity);
                                messageParameters.Add("RCC", product.AssociatedWithCombo ? "Y" : "N");
                                messageParameters.Add("USC", campaign.RecordInsertionUser);
                                messageParameters.Add("PSA", state.Abbreviation);

                                var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));

                                var authorizerRepository = campaignConfigurationFactory.Create(environmentType).CreateConnection();
                                await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());

                                if (environmentType.Equals(CampaignEnvironmentType.Homolog))
                                {
                                    await connectionContext.QueryAsync<string>(sql, new ProductEntity()
                                    {
                                        City_Code = city.Id,
                                        Mileage = 0,
                                        MultiPrd_Id = product.DiscountType,
                                        Prc_AttendInfo = string.Empty,
                                        Prc_CurrentBillDesc = 0,
                                        Prd_CurrentBillQty = 0,
                                        Prc_Desc = product.Discount,
                                        Prc_PercCo = 0,
                                        Prc_PercIssuer = 0,
                                        Prd_QtyMax = maxQuantity,
                                        Prd_StdCode = Convert.ToDecimal(product.EAN),
                                        Rec_InsTime = DateTime.Now,
                                        Rec_InsUser = campaign.RecordInsertionUser,
                                        Rec_UpdTime = DateTime.Now,
                                        Rec_St = 'A',
                                        Rec_StInfo = string.Empty,
                                        Rec_UpdUser = campaign.RecordInsertionUser,
                                        Rules_BeginDate = campaign.BeginDate,
                                        Rules_Code = campaign.Id,
                                        Rules_ComboId = product.AssociatedWithCombo ? 'Y' : 'N',
                                        Trn_ProfessType = 0,
                                        State = state.Abbreviation

                                    });
                                }
                                  

                                product.Campaign = new Campaign { Id = campaign.Id, BeginDate = campaign.BeginDate };
                            }
                        }
                    }
                }
            } 

        }

        public async Task<Brand[]> GetProductBrands(ProductSearchParameters parameters)
        {
            var brands = new List<Brand>();
            var productDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
            using (var connectionContext = productDataContext.AcquireConnection())
            {
                var sql = @"SELECT DISTINCT PRM.Prd_Brand [Id], 
                                   RTRIM(MT.Table_Info1) [Name]
                                  from [TRNCENTRE_ADV_V7].[dbo].Prd_MsFile prm INNER JOIN
	                        [TRNCENTRE_ADV_V7].[dbo].Str_Multi_Table mt
	                        on prm.Prd_Brand = mt.Table_KeyCode
                                   WHERE 
	                                        PRM.Rec_St = 'A' 
					                        AND PRM.Acquirer_Code = @AcquirerCode
					                        AND MT.Acquirer_Code = @AcquirerCode
					                        AND RTRIM(PRM.Prd_Brand) <> ''
			                                AND mt.Table_KeyType = 'Prd_Brand' 
					                        AND prm.Prd_Brand NOT IN 
					                        (
						                        SELECT Prd_Brand 
						                        FROM [TRNCENTRE_ADV_V7].[dbo].Pub_Str_Holder_Questions
						                        WHERE 
							                        [TRNCENTRE_ADV_V7].[dbo].Pub_Str_Holder_Questions.Acquirer_Code = @AcquirerCode
					                        )

	                        ORDER BY  RTRIM(MT.Table_Info1)";
                var result = await connectionContext.QueryAsync<Brand>(sql, new { AcquirerCode = parameters.AcquirerCode });
                if (result != null && result.Any())
                {
                    brands = result.ToList();
                    using (var connectionContext2 = dataContext.AcquireConnection())
                    {
                        var sql2 = @"SELECT Prd_Brand [BrandId]
						FROM Pub_Str_Holder_Questions
						WHERE 
							Pub_Str_Holder_Questions.Acquirer_Code = @AcquirerCode";

                        var draftBrands = (await connectionContext2.QueryAsync<string>(sql2, new { AcquirerCode = parameters.AcquirerCode })).Select(
                            b => Convert.ToInt32(b)
                            ).ToArray();
                                                                     
                        brands = brands.Where(x => !draftBrands.Any(y => y == x.Id)).ToList();
                    }
                }
            }
          

            return brands.ToArray();
        }

        public async Task<Brand> GetProductBrandByProduct(string ean)
        {
            Brand brand = null;
            var productDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
            using(var connectionConext = productDataContext.AcquireConnection())
            {
                var sql = @"SELECT DISTINCT TOP 1
			                    PRM.Prd_Brand [Id]
			                    ,RTRIM(MT.Table_Info1) [Name] 
                              FROM [TRNCENTRE_ADV_V7].[dbo].Prd_MsFile prm INNER JOIN
		                       [TRNCENTRE_ADV_V7].[dbo].Str_Multi_Table mt
		                      on prm.Prd_Brand = mt.Table_KeyCode
                               WHERE 
	                                    PRM.Rec_St = 'A' 
			                            AND mt.Table_KeyType = 'Prd_Brand'
					                    AND PRM.Prd_StdCode = @EAN
					                    AND RTRIM(PRM.Prd_Brand) <> ''";

                var result = await connectionConext.QueryAsync<Brand>(sql, new { EAN = ean });
                if(result != null && result.Any())
                {
                    brand = result.First();
                }
            }
          


            return brand;
        }

        public async Task<string> GetProductDescriptionByEAN(int acquirerCode, string ean)
        {
            var description = string.Empty;
            var productDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
            using(var connectionContext = productDataContext.AcquireConnection())
            {
                var sql = @"select top 1
			                        cast(prm.Prd_StdCode as VARCHAR(15)) +  ' - '+ RTRIM(prm.Prd_Name)+ SPACE(1)
			                        + RTRIM(prm.Prd_Dose)
			                        + RTRIM(prm.Prd_PutUp)
			                        + RTRIM(prm.Prd_Pharma) 
			                        + SPACE(1)
			                        + 'Apresentação' COLLATE Latin1_General_100_CI_AS_SC [Description]
	                        from [TRNCENTRE_ADV_V7].[dbo].Prd_MsFile prm
                            WHERE
		                            prm.Acquirer_Code = @AcquirerCode  AND
	                                prm.Prd_StdCode = @EAN";

                var result = await connectionContext.QueryAsync<string>(sql, new { AcquirerCode = acquirerCode, EAN = ean });

                if(result != null && result.Any())
                {
                    return result.First();
                }
            }
          


            return description;
        }

        public async Task<Product[]> GetProductsByCampaign(Campaign campaign)
        {
            var products = new List<Product>();
            var productDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
            using(var connectionContext = productDataContext.AcquireConnection())
            {
                var sql = @"SELECT 
                             Product.Rules_ComboId [AssociatedWithCombo]
                             ,Product.Prc_Desc [Discount]
                             ,Product.MultiPrd_Id [DiscountType]
                             ,Product.Prd_StdCode [EAN]
                             ,Product.[State] 
                             ,Product.City_Code [City]
                             ,RTRIM([State].State_Name) [StateName]
                             ,Product.Prd_QtyMax [MaxQuantity]
                             FROM [TRNCENTRE_ADV_V7].[dbo].Rules_Prd Product INNER JOIN
								[TRNCENTRE_ADV_V7].[dbo].Str_ZIP_State [State] ON Product.[State] = [State].[State]
                             WHERE
	                            Product.Rules_Code = @Id AND
								Product.Rules_BeginDate = @BeginDate AND 
								Product.Rec_St IN ('A','D')";

                var result = await connectionContext.QueryAsync<dynamic>(sql, new { Id = campaign.Id, BeginDate = campaign.BeginDate });

                if(result != null && result.Any())
                {
                    products = result.ToList().Select(p => new Product()
                    {
                        AssociatedWithCombo = p.AssociatedWithCombo.ToString().Equals("Y") ? true : false,
                        Discount = Convert.ToDecimal(p.Discount.ToString(), CultureInfo.InvariantCulture),
                        DiscountType = p.DiscountType.ToString()[0],
                        EAN = p.EAN.ToString(),
                        MaxQuantity = short.Parse(p.MaxQuantity.ToString())
                    }).Distinct(new ProductComparer()).ToList();

                    if (products != null && products.Any())
                    {
                        foreach (var p in products)
                        {
                            p.States = result.ToList().Where(s => s.EAN.ToString() == p.EAN).Select(state => new State()
                            {
                                Abbreviation = state.State.ToString(),
                                Name = state.StateName.ToString(),
                                Cities = result.ToList().Where(c => c.EAN.ToString() == p.EAN && c.State.ToString() == state.State.ToString()).Select(city => new
                                City()
                                { Id = Convert.ToInt32(city.City.ToString()) }).ToArray()
                            }).Distinct(new StateComparer()).ToArray();
                            if (p.States != null && p.States.Count() > 0)
                            {
                                foreach (var s in p.States)
                                {
                                    foreach (var city in s.Cities)
                                    {
                                        if (city.Id.Equals(0))
                                        {
                                            city.TotalQuantity = await cityRepository.GetTotalCityCountByState(s.Abbreviation);
                                        }
                                    }
                                }
                            }

                            p.Brand = await GetProductBrandByProduct(p.EAN);
                            p.Description = await GetProductDescriptionByEAN(campaign.AcquirerCode, p.EAN);
                            var fullbox = await GetProductFullBox(new ProductSearchParameters(campaign.AcquirerCode, p.EAN, false));
                            var maxquantity = p.MaxQuantity * fullbox;
                            if (campaign.LimitMode > 0)
                                maxquantity = maxquantity / campaign.LimitMode;

                            p.MaxQuantity = Convert.ToInt32(maxquantity);
                        }
                    }
                }

               
            }

            return products.ToArray();
        }

        public async Task<Brand> GetBrandById(int id)
        {
            var productDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
            using(var connectionContext = productDataContext.AcquireConnection())
            {
                var sql = @"SELECT TOP 1 PRM.Prd_Brand [Id], RTRIM(MT.Table_Info1) [Name]
                                  from [TRNCENTRE_ADV_V7].[dbo].Prd_MsFile prm INNER JOIN
	                        [TRNCENTRE_ADV_V7].[dbo].Str_Multi_Table mt
	                        on prm.Prd_Brand = mt.Table_KeyCode
                                   WHERE 
	                                        PRM.Rec_St = 'A' 
				                            AND PRM.Prd_Brand = @BrandId
					                        AND RTRIM(PRM.Prd_Brand) <> ''
			                                AND mt.Table_KeyType = 'Prd_Brand' ";

                var result = await connectionContext.QueryAsync<Brand>(sql, new { BrandId = id });
                if(result !=null && result.Any())
                {
                    return result.First();
                }
            }

            return null;
        }
    }
}
