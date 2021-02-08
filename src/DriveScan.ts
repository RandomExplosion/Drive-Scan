import Electron from "electron";

export class DriveScan {
    public win: Electron.BrowserWindow;

    /** Start the application, this is the public entrypoint */
    public static start(app: Electron.App): void {
        app.on("ready", () => new DriveScan(app));
    }
    
    /** The private constructor actually starts the program */
    private constructor(public app: Electron.App) {
        this.win = new Electron.BrowserWindow({
            webPreferences: {
                nodeIntegration: true,
                enableRemoteModule: true,
            },
        });

        // Open the devtools
        this.win.webContents.openDevTools();

        // Load the index file
        this.win.loadFile("index.html");
    }
}