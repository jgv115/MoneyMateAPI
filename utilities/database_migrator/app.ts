import * as readline from 'node:readline/promises';
import { stdin as input, stdout as output } from 'node:process';
import { Environment, MigrationType } from './constants';
import {
    CockroachDbTargetUserRepository,
} from "./repository/cockroachdb/cockroachdb_user_repository";
import { Pool } from 'pg';
import { getCockroachDbConfig } from './config/cockroachdb_config_provider';
import { DynamoDbSourceUserRepository } from './repository/dynamodb/dynamodb_user_repository';
import { MigrationHandler } from './migration_handler/migration_handler';
import { UserMigrationHandler } from './migration_handler/user_migration_handler';
import { createLogger } from './utils/logger';
import { CockroachDbTargetCategoryRepository } from './repository/cockroachdb/cockroachdb_category_repository';
import { CockroachDbTargetPayerPayeeRepository } from './repository/cockroachdb/cockroachdb_payerpayee_repository';
import { CockroachDbTransactionRepository } from './repository/cockroachdb/cockroachdb_transaction_repository';
import { DynamoDBClient } from "@aws-sdk/client-dynamodb";


const setupDependencies = async (migrationType: MigrationType, sourceEnvironment: Environment, targetEnvironment: Environment) => {
    const logger = createLogger();

    logger.info("setting up dependencies")

    const cockroachDbConfig = getCockroachDbConfig(sourceEnvironment);

    const cockroachDbConnection = new Pool({
        host: cockroachDbConfig.host,
        port: cockroachDbConfig.port,
        database: cockroachDbConfig.database,
        user: cockroachDbConfig.user,
        password: cockroachDbConfig.password,
        max: 20,
        idleTimeoutMillis: 10000,
        connectionTimeoutMillis: 2000
    });

    const dynamoDbClient = new DynamoDBClient();

    const sourceUserRepository = DynamoDbSourceUserRepository(logger, dynamoDbClient, { tableName: "gfjkdgfdjk" });

    const targetUserRepository = CockroachDbTargetUserRepository(logger, cockroachDbConnection)
    const targetCategoryRepository = CockroachDbTargetCategoryRepository(logger, cockroachDbConnection);
    const targetPayerPayeeRepository = CockroachDbTargetPayerPayeeRepository(logger, cockroachDbConnection);
    const targetTransactionRepository = CockroachDbTransactionRepository(logger, cockroachDbConnection);

    var migrationHandler: MigrationHandler;

    switch (migrationType) {
        case MigrationType.user: {
            migrationHandler = UserMigrationHandler(logger, sourceUserRepository, targetUserRepository)
            break;
        }
        default:
            throw Error("unsupported migration type")
    }

    return {
        migrationHandler
    }
}

const startDbMigration = async (migrationType: MigrationType, sourceEnvironment: Environment, targetEnvironment: Environment) => {
    const { migrationHandler } = await setupDependencies(migrationType, sourceEnvironment, targetEnvironment);

    await migrationHandler.handleMigration();

}

const main = async () => {
    const rl = readline.createInterface({ input, output });

    const migrationType: MigrationType = MigrationType[await rl.question('What type of migration would you like to perform: user, category, payerpayee, transaction? ')];
    if (migrationType === undefined)
        throw Error("unsupported migration type")


    const sourceEnvironment: Environment = Environment[await rl.question("Source environment? local, test, prod? ")];
    if (sourceEnvironment === undefined)
        throw Error("unsupported environment")

    const targetEnvironment: Environment = Environment[await rl.question("Source environment? local, test, prod? ")];
    if (targetEnvironment === undefined)
        throw Error("unsupported environment")

    console.log(migrationType.toString())
    console.log(sourceEnvironment);
    console.log(targetEnvironment);
    rl.close();

    await startDbMigration(migrationType, sourceEnvironment, targetEnvironment)
}

startDbMigration(MigrationType.user, Environment.local, Environment.test);
// main();