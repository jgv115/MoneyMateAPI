import winston from "winston"

export const createLogger = () =>
    winston.createLogger(
        {
            level: "info",
            format: winston.format.combine(winston.format.align(), winston.format.colorize(), winston.format.simple()),
            transports: [
                new winston.transports.Console
            ]
        }
    )
