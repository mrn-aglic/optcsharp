const path = require('path');
const webpack = require('webpack');
const VueLoaderPlugin = require('vue-loader/lib/plugin');
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');
const MonacoWebpackPlugin = require('monaco-editor-webpack-plugin');

module.exports = {
    mode: process.env.NODE_ENV,
    devtool: 'source-map',
    entry: {
        main: './wwwroot/client/src/main.js',
    },
    output: {
        path: path.resolve(__dirname, './wwwroot/vuebundles/'),
        publicPath: '/vuebundles/',
        filename: '[name].build.js',
    },
    module: {
        rules: [
            {
                test: /\.css$/,
                use: [
                    'vue-style-loader',
                    'style-loader',
                    'css-loader'
                ],
            }, {
                test: /\.vue$/,
                loader: 'vue-loader'
            },
            {
                test: /\.js$/,
                loader: 'babel-loader',
                exclude: /node_modules/
            },
            {
                test: /\.(png|jpg|gif|svg|ttf)$/,
                loader: 'file-loader',
                options: {
                    name: '[name].[ext]?[hash]'
                }
            },
        ]
    },
    plugins: [
        new VueLoaderPlugin(),
        new MonacoWebpackPlugin({
            languages: ['javascript', 'csharp']
        }),
        // new webpack.optimize.LimitChunkCountPlugin({
        //     maxChunks: 1
        // })
    ]
};
