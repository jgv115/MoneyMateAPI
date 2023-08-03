import { Category } from "../model";

export interface TargetCategoryRepository {
    saveCategories: (categories: Category[]) => Promise<string[]>;
    getSubcategoryIdFromSubcategoryName: (userId: string, subcategoryName: string) => Promise<string>;
}