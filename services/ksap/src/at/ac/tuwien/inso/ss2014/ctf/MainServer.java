package at.ac.tuwien.inso.ss2014.ctf;

import java.io.IOException;
import java.sql.SQLException;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import at.ac.tuwien.inso.ss2014.ctf.cli.ServerCli;
import at.ac.tuwien.inso.ss2014.ctf.dao.SQLiteDBConnection;

public class MainServer {

	final static Logger logger = LoggerFactory.getLogger(MainServer.class);

	/**
	 * @param args
	 *            <port number> | 0 < port number < 65536");
	 */
	public static void main(String[] args) {
		logger.info("Entering application");

		if (args.length == 1) {
			if (args[0]
					.matches("(6553[0-5]|655[0-2][0-9]\\d|65[0-4](\\d){2}|6[0-4](\\d){3}|[1-5](\\d){4}|[1-9](\\d){0,3})")) {
				try {
					// Test DB Connection (includes a possible regeneration of
					// the DB...)
					logger.info("Checking database connection and integrity...");
					logger.info("#############################################");

					SQLiteDBConnection.getReadOnlyConnection().close();

					logger.info("#############################################\n");
					logger.info("Database successully working!\n");

					new ServerCli(args);

				} catch (SQLException e) {
					e.printStackTrace();
					logger.error(e.getMessage());
				} catch (ClassNotFoundException e) {
					e.printStackTrace();
					logger.error(e.getMessage());
				} catch (IOException e) {
					e.printStackTrace();
					logger.error(e.getMessage());
				}
			} else {
				logger.error("Impossible port number! Please specify a port between 1 and 65535");
			}
		} else {
			logger.error("Wrong usage! Possible arguments: <port number> | 0 < port number < 65536");
		}

		// System.exit(0);
	}
}