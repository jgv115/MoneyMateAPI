import { Pool } from "pg";
import { Logger } from "winston";
import { TargetCategoryRepository } from "../target_category_repository";
import { Category } from "../../model";

export const CockroachDbTargetCategoryRepository = (logger: Logger, client: Pool): TargetCategoryRepository => {

    const saveCategories = async (userId: string, categories: Category[]): Promise<void> => {
        for (const category of categories) {
            await client.query(`INSERT INTO CATEGORY (user_identifier) VALUES ($1, $2, $3)`, [category.name, userId, category.transactionType]);

            for (const subcategory of category.subcategories) {
                
            }
        }
    }

    return {
        saveCategories
    }
}