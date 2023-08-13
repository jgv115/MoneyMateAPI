import { Pool } from "pg";
import { Logger } from "winston";
import { Category } from "../../model";

export const CockroachDbTargetCategoryRepository = (logger: Logger, client: Pool) => {

    const saveCategories = async (categories: Category[]): Promise<string[]> => {
        const savedCategoryIds = [];

        for (const category of categories) {
            logger.info(`Saving category ${category.name} with subcategories: ${category.subcategories}`);
            const response = await client.query(`
            INSERT
            INTO CATEGORY (name, user_id, transaction_type_id) 
            SELECT $1, $2, tt.id
            FROM transactiontype tt WHERE tt.name = $3
            RETURNING id
            `, [category.name, category.userId, category.transactionType]);

            const categoryId = response.rows[0].id;

            savedCategoryIds.push(categoryId);

            for (const subcategory of category.subcategories) {
                await client.query(`
                INSERT
                INTO subcategory (name, category_id)
                VALUES ($1, $2)`, [subcategory, categoryId])
            }
        }

        return savedCategoryIds;
    }


    const getSubcategoryId = async (userId: string, categoryId: string, subcategoryName: string): Promise<string> => {
        const response = await client.query(
            `SELECT subcategory.id from subcategory
            LEFT JOIN category on category.id = subcategory.category_id
            WHERE subcategory.name = $1 and category.user_id = $2 and category.id = $3`, [subcategoryName, userId, categoryId]);

        return response.rows[0].id;
    }

    return {
        saveCategories,
        getSubcategoryId
    }
}