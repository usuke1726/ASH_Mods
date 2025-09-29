
const path = require("path");
const TerserPlugin = require("terser-webpack-plugin");

module.exports = {
    entry: './index.js',
    mode: "production",
    target: "node",
    optimization: {
        minimizer: [new TerserPlugin({
            terserOptions: {
                format: {
                    ascii_only: true,
                },
            },
        })],
    },
    output: {
        path: path.resolve(__dirname, "dist"),
        filename: "server.js",
    },
};

