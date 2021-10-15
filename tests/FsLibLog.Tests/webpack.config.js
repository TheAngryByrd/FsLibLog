var path = require("path");

module.exports = {
    mode: "development",
    entry: "./FsLibLog.Tests.fsproj",
    output: {
        path: path.join(__dirname, "./public"),
        filename: "bundle.js",
    },
    devServer: {
        static: {
            directory: path.join(__dirname, 'public'),
        },
        port: 9000,
    },
    module: {
        rules: [{
            test: /\.fs(x|proj)?$/,
            use: "fable-loader"
        }]
    }
}
