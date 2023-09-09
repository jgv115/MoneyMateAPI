import { Pool } from "pg";
import { Logger } from "winston";
import { CockroachDbCategory } from "./model";


export type CockroachDbTargetCategoryRepository = ReturnType<typeof CockroachDbTargetCategoryRepositoryBuilder>

export const CockroachDbTargetCategoryRepositoryBuilder = (logger: Logger, client: Pool) => {

    const saveCategory = async (category: CockroachDbCategory): Promise<string> => {

        logger.info(`Saving category ${category.name} with subcategories: ${JSON.stringify(category.subcategories)}`);
        const response = await client.query(`
            INSERT
            INTO CATEGORY (name, user_id, transaction_type_id) 
            SELECT $1, $2, $3
            RETURNING id
            `, [category.name, category.user_id, category.transaction_type_id]);

        const categoryId = response.rows[0].id;


        for (const subcategory of category.subcategories) {
            await client.query(`
                INSERT
                INTO subcategory (name, category_id)
                VALUES ($1, $2)`, [subcategory, categoryId])
        }

        return categoryId;
    }

    const saveCategories = async (categories: CockroachDbCategory[]): Promise<string[]> => {
        const savedCategoryIds = [];

        for (const category of categories) {
            const savedCategoryId = await saveCategory(category);
            savedCategoryIds.push(savedCategoryId);
        }

        return savedCategoryIds;
    }


    const getSubcategoryIdWithCategoryId = async (userId: string, categoryId: string, subcategoryName: string): Promise<string> => {
        const response = await client.query(
            `SELECT subcategory.id from subcategory
            LEFT JOIN category on category.id = subcategory.category_id
            WHERE subcategory.name = $1 and category.user_id = $2 and category.id = $3`, [subcategoryName, userId, categoryId]);

        return response.rows[0].id;
    };

    const getSubcategoryIdByCategoryAndSubcategoryName = async (
        userId: string,
        categoryName: string,
        subcategoryName: string,
        transactionTypeId: string
    ): Promise<string> => {
        const response = await client.query(
            `SELECT subcategory.id from subcategory
            LEFT JOIN category on category.id = subcategory.category_id
            WHERE subcategory.name = $1 and category.name = $2 and category.transaction_type_id = $3 and category.user_id = $4`,
            [subcategoryName, categoryName, transactionTypeId, userId]
        );

        if (!response.rows[0]?.id) {
            logger.error("unable to find subcategoryId given the inputs", { userId, categoryName, subcategoryName, transactionTypeId });
            return undefined;
        }

        return response.rows[0].id;
    }

    return {
        saveCategory,
        saveCategories,
        getSubcategoryIdWithCategoryId,
        getSubcategoryIdByCategoryAndSubcategoryName
    }
}