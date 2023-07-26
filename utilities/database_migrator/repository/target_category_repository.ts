import { Category } from "../model";

export interface TargetCategoryRepository {
    saveCategories: (userId: string, categories: Category[]) => Promise<void>
}