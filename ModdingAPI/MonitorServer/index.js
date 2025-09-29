
const DEBUG = false;
const defaultPort = 8080;
function GetPort(){
    if(process.argv.length < 3) return defaultPort;
    const port = parseInt(process.argv[2]);
    if(!Number.isInteger(port)) return defaultPort;
    if(port >= 1024 && port <= 65535) return port;
    if(port === 80) return port;
    return defaultPort;
}
const port = GetPort();
const { WebSocketServer } = require("ws");
const ws = new WebSocketServer({
    port,
});
console.log(`ModdingAPI Monitor Server (listening port ${port})`);
ws.on("connection", socket => {
    DEBUG && console.log("websocket connected");
    socket.on("message", rawData => {
        try{
            console.log(rawData.toString("utf8").replaceAll("\\x1b[", "\x1b["));
        }catch(e){
            console.log(`Error: ${e}`);
        }
    });
    socket.on("error", () => {});
    socket.on("close", () => {
        DEBUG && console.log("connection closed");
        ws.close();
    });
});
ws.on("error", () => {});
