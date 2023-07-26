import { Environment } from "../constants";

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
            return {
                host: "moneymate-cluster-5098.6xw.cockroachlabs.cloud",
                port: 26257,
                database: "moneymate_db_local",
                user: "root",
                password: "",
            }
        }
    }
};

