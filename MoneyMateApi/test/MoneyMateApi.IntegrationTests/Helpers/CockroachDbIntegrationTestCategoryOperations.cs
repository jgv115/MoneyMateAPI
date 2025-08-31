using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MoneyMateApi.Constants;
using MoneyMateApi.Domain.Categories;
using MoneyMateApi.Repositories;
using MoneyMateApi.Repositories.CockroachDb;

namespace MoneyMateApi.IntegrationTests.Helpers;

public class CockroachDbIntegrationTestCategoryOperations
{
    private readonly ICategoriesRepository _categoriesRepository;
    private readonly CockroachDbIntegrationTestTransactionTypeOperations _transactionTypeOperations;
    private readonly DapperContext _dapperContext;
    private readonly Guid _testUserId;

    public CockroachDbIntegrationTestCategoryOperations(
        DapperContext dapperContext,
        ICategoriesRepository categoriesRepository,
        Guid testUserId, CockroachDbIntegrationTestTransactionTypeOperations transactionTypeOperations)
    {
        _dapperContext = dapperContext;
        _categoriesRepository = categoriesRepository;
        _testUserId = testUserId;
        _transactionTypeOperations = transactionTypeOperations;
    }

    public async Task<Dictionary<Guid, List<Guid>>> WriteCategoriesIntoDb(List<Category> categories)
    {
        var transactionTypeIds = _transactionTypeOperations.GetTransactionTypeIds();
        
            var categoryAndSubcategoryIdMap = new Dictionary<Guid, List<Guid>>();
        using (var connection = _dapperContext.CreateConnection())
        {
            foreach (var category in categories)
            {
                var categoryId = await WriteCategoryIntoDb(connection, category, transactionTypeIds);

                foreach (var subcategory in category.Subcategories)
                {
                    var subcategoryId = await WriteSubcategoryIntoDb(connection, categoryId, subcategory);

                    var subcategories = categoryAndSubcategoryIdMap.GetValueOrDefault(categoryId, new List<Guid>());
                    subcategories.Add(subcategoryId);
                    categoryAndSubcategoryIdMap[categoryId] = subcategories;
                }
            }
        }

        return categoryAndSubcategoryIdMap;
    }

    public async Task<Guid> WriteCategoryIntoDb(IDbConnection connection, Category category,
        TransactionTypeIds transactionTypeIds)
    {
        var createCategoryQuery =
            @"
                WITH e AS (
                    INSERT INTO category (name, user_id, transaction_type_id, profile_id) 
                        VALUES (@category_name, @user_id, @transaction_type_id, @profile_id)
                        ON CONFLICT (name, profile_id, transaction_type_id) DO NOTHING
                        RETURNING id
                )
                SELECT * FROM e
                UNION
                SELECT id FROM category c WHERE c.name = @category_name
                    AND c.user_id = @user_id
                    AND c.transaction_type_id = @transaction_type_id;               
            ";

        var categoryId = await connection.QuerySingleAsync<Guid>(createCategoryQuery,
            new
            {
                category_name = category.CategoryName, user_id = _testUserId,
                transaction_type_id = category.TransactionType == TransactionType.Expense
                    ? transactionTypeIds.Expense
                    : transactionTypeIds.Income,
                profile_id = _testUserId
            });

        return categoryId;
    }

    public async Task<Guid> WriteSubcategoryIntoDb(IDbConnection connection, Guid categoryId, string subcategoryName)
    {
        var subcategoryQuery =
            @"
                WITH e AS (
                    INSERT INTO subcategory (name, category_id)
                    VALUES (@subcategoryName, @categoryId)
                    ON CONFLICT (name, category_id) DO UPDATE SET name = excluded.name
                    RETURNING id
                )
                SELECT * FROM e
                UNION
                SELECT id FROM subcategory sc WHERE sc.category_id = @categoryId AND sc.name = @subcategoryName;
            ";
        return await connection.QuerySingleAsync<Guid>(subcategoryQuery,
            new {subcategoryName, categoryId});
    }


    public async Task<IEnumerable<Category>> RetrieveAllCategories()
    {
        return (await _categoriesRepository.GetAllCategories()).ToList();
    }

    public async Task<Category?> RetrieveCategory(string categoryName)
    {
        return (await _categoriesRepository.GetAllCategories()).FirstOrDefault(category =>
            category.CategoryName == categoryName);
    }
}