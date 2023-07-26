export interface TargetUserRepository {
    getUserIdFromUserIdentifier: (userIdentifier: string) => Promise<string>
    saveUsers: (userIdentifiers: string[]) => Promise<void>
}