import { Pool } from "pg";
import { Logger } from "winston";
import { CockroachDbCategory } from "./model";


export type CockroachDbTargetCategoryRepository = ReturnType<typeof CockroachDbTargetCategoryRepositoryBuilder>

export const CockroachDbTargetCategoryRepositoryBuilder = (logger: Logger, client: Pool) => {

    const saveCategories = async (categories: CockroachDbCategory[]): Promise<string[]> => {
        const savedCategoryIds = [];

        for (const category of categories) {
            logger.info(`Saving category ${category.name} with subcategories: ${category.subcategories}`);
            const response = await client.query(`
            INSERT
            INTO CATEGORY (name, user_id, transaction_type_id) 
            SELECT $1, $2, $3
            RETURNING id
            `, [category.name, category.user_id, category.transaction_type_id]);

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