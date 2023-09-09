import { Environment } from "../constants";
import 'dotenv/config';

export interface CockroachDbConfig {
    host: string
    port: number
    database: string
    user: string
    password: string
}

export const getCockroachDbConfig = (environment: Environment): CockroachDbConfig => {
    switch (environment) {
        case Environment.local: {
            return {
                host: "localhost",
                port: 26257,
                database: "moneymate_db_local",
                user: "root",
                password: "",
            }
        }
        default: {
            const cockroachDbPassword = process.env.COCKROACH_DB_PASSWORD;

            if (!cockroachDbPassword)
                throw new Error("CockroachDb password not found. Please set the cockroachDbPassword environment variable")

            return {
                host: "moneymate-cluster-5098.6xw.cockroachlabs.cloud",
                port: 26257,
                database: `moneymate_db_${environment}`,
                user: "jgv115",
                password: cockroachDbPassword,
            }
        }
    }
};

