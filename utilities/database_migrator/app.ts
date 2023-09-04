import * as readline from 'node:readline/promises';
import { stdin as input, stdout as output } from 'node:process';
import { Environment, MigrationType } from './constants';
import {
    CockroachDbTargetUserRepositoryBuilder,
} from "./repository/cockroachdb/cockroachdb_user_repository";
import { Pool } from 'pg';
import { getCockroachDbConfig } from './config/cockroachdb_config_provider';
import { DynamoDbSourceUserRepositoryBuilder } from './repository/dynamodb/dynamodb_user_repository';
import { MigrationHandler } from './migration_handler/migration_handler';
import { UserMigrationHandler } from './migration_handler/user_migration_handler';
import { createLogger } from './utils/logger';
import { CockroachDbTargetCategoryRepositoryBuilder } from './repository/cockroachdb/cockroachdb_category_repository';
import { CockroachDbTargetPayerPayeeRepositoryBuilder } from './repository/cockroachdb/cockroachdb_payerpayee_repository';
import { CockroachDbTargetTransactionRepositoryBuilder } from './repository/cockroachdb/cockroachdb_transaction_repository';
import { DynamoDBClient } from "@aws-sdk/client-dynamodb";
import { CategoryMigrationHandler } from './migration_handler/category_migration_handler';
import { DynamoDbMoneyMateDbRepositoryBuilder } from './repository/dynamodb/dynamodb_moneymate_repository';
import { PayerPayeeMigrationHandler } from './migration_handler/payer_payee_migration_handler';
import { TransactionMigrationHandler } from './migration_handler/transaction_migration_handler';


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

    const sourceUserRepository = DynamoDbSourceUserRepositoryBuilder(logger, dynamoDbClient, { tableName: "gfjkdgfdjk" });
    const sourceMoneyMateDbRepository = DynamoDbMoneyMateDbRepositoryBuilder(logger, dynamoDbClient, { tableName: "gfdjk" });

    const targetUserRepository = CockroachDbTargetUserRepositoryBuilder(logger, cockroachDbConnection)
    const targetCategoryRepository = CockroachDbTargetCategoryRepositoryBuilder(logger, cockroachDbConnection);
    const targetPayerPayeeRepository = CockroachDbTargetPayerPayeeRepositoryBuilder(logger, cockroachDbConnection);
    const targetTransactionRepository = CockroachDbTargetTransactionRepositoryBuilder(logger, cockroachDbConnection);

    let migrationHandler: any;

    switch (migrationType) {
        case MigrationType.user: {
            migrationHandler = UserMigrationHandler(logger, sourceUserRepository, targetUserRepository)
            break;
        }
        case MigrationType.category: {
            migrationHandler = CategoryMigrationHandler(logger, sourceMoneyMateDbRepository, targetCategoryRepository, sourceUserRepository, targetUserRepository, targetTransactionRepository);
            break;
        }
        case MigrationType.payerpayee: {
            migrationHandler = PayerPayeeMigrationHandler(logger, sourceMoneyMateDbRepository, targetPayerPayeeRepository, sourceUserRepository, targetUserRepository);
            break;
        }
        case MigrationType.transaction: {
            migrationHandler = TransactionMigrationHandler(logger, sourceMoneyMateDbRepository, targetTransactionRepository, sourceUserRepository, targetUserRepository, targetCategoryRepository);
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