import { Category } from "../model";

export interface SourceCategoryRepository {
    getAllCategories: () => Promise<Category[]>
}