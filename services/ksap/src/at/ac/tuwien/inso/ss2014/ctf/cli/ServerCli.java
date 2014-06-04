package at.ac.tuwien.inso.ss2014.ctf.cli;

import java.io.IOException;
import java.sql.SQLException;
import java.util.Scanner;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import at.ac.tuwien.inso.ss2014.ctf.conn.RpcNetworkService;
import at.ac.tuwien.inso.ss2014.ctf.dao.SQLiteDBConnection;

public class ServerCli {

	private final Logger logger = LoggerFactory.getLogger(this.getClass());

	private int port;
	private RpcNetworkService nwService;

	public ServerCli(String[] args) throws SQLException {
		try {
			this.port = Integer.parseInt(args[0]);

			boolean running = true;

			nwService = new RpcNetworkService(port);

			nwService.run(); // For interacting with Server, use command
								// nwService.start(); instead!

			Scanner sc = new Scanner(System.in);

			logger.info("----------------------");
			logger.info("Initialized CTF Server");
			logger.info("----------------------");
			logger.info("");

			String command = "";

			while (running && sc.hasNextLine()) {
				command = sc.nextLine();

				if (command.equalsIgnoreCase("!exit")) {
					logger.info("Server shutdown initialized...");
					running = false;
					shutdown();
					break;

				} else if (command.equalsIgnoreCase("!close")) {
					// Close all client connections and make server
					// unreachable

					if (nwService != null)
						nwService.closeConnection();

				} else if (command.equalsIgnoreCase("!reconnect")) {
					// reconnects network (after !close)

					if (!nwService.isAlive()) {
						logger.info("Reopening network connection...");
						nwService = new RpcNetworkService(port);
						nwService.start();
					} else
						System.out
								.println("Network connection is still established...");
				}
			}
			sc.close();

		} catch (NumberFormatException e) {
			logger.error(e.getMessage());
			shutdown();
		} catch (IOException e) {
			logger.error(e.getMessage());
			shutdown();
		}
	}

	private void shutdown() {
		logger.info("------------------------");
		logger.info("Shutting down CTF Server");
		logger.info("------------------------");
		logger.info("");

		try {
			SQLiteDBConnection.shutdown();
		} catch (SQLException e) {
			logger.error(e.getMessage());
		}

		try {
			if (nwService != null) {
				nwService.shutdown();
			}
		} catch (IOException e) {
			logger.error(e.getMessage());
		}
	}
}