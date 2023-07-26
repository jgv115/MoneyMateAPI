export interface SourceUserRepository {
    getAllUserIdentifiers: () => Promise<string[]>
}