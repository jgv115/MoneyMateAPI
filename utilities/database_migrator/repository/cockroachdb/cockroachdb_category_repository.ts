import { Pool } from "pg";
import { Logger } from "winston";
import { TargetCategoryRepository } from "../target_category_repository";
import { Category } from "../../model";

export const CockroachDbTargetCategoryRepository = (logger: Logger, client: Pool): TargetCategoryRepository => {

    const saveCategories = async (categories: Category[]): Promise<string[]> => {
        const savedCategoryIds = [];

        for (const category of categories) {
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

    const getSubcategoryIdFromSubcategoryName = async (userId: string, subcategoryName: string): Promise<string> => {
        const response = await client.query(
            `SELECT subcategory.id from subcategory
            LEFT JOIN category on category.id = subcategory.category_id
            WHERE subcategory.name = $1 and category.user_id = $2`, [subcategoryName, userId]);

        return response.rows[0].id;
    }

    return {
        saveCategories,
        getSubcategoryIdFromSubcategoryName
    }
}