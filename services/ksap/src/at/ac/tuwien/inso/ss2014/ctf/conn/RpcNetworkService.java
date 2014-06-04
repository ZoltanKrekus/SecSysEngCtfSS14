package at.ac.tuwien.inso.ss2014.ctf.conn;

import java.io.IOException;
import java.net.ServerSocket;
import java.net.SocketException;
import java.util.Vector;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class RpcNetworkService extends Thread {

	private final Logger logger = LoggerFactory.getLogger(this.getClass());

	private ServerSocket serverSocket;
	private ExecutorService pool;
	private boolean running = true, alreadyShutdown = true;
	private Vector<RpcHandler> handlerList = new Vector<RpcHandler>();
	private RpcHandler handler;
	private int port;

	public Vector<RpcHandler> getHandlerList() {
		return handlerList;
	}

	public synchronized void setHandlerList(Vector<RpcHandler> handlerList) {
		this.handlerList = handlerList;
	}

	public RpcNetworkService(int port) throws IOException {
		this.port = port;
		this.serverSocket = new ServerSocket(this.port, 1000);
		logger.info("Bound Server on {}",
				this.serverSocket.getLocalSocketAddress());
		this.pool = Executors.newCachedThreadPool();
	}

	@Override
	public void run() {

		try {
			while (running) {
				handler = new RpcHandler(serverSocket.accept());
				handlerList.add(handler);
				this.pool.execute(handler);
			}

			shutdown();

		} catch (IOException e) {
			if (!(!running && e.getClass() == SocketException.class)) {

			}
		}
	}

	/**
	 * Oracle Example, which you linked on the VS-lab homepage
	 * http://docs.oracle
	 * .com/javase/6/docs/api/index.html?java/util/concurrent/ExecutorService
	 * .html
	 * 
	 * @param Pool
	 * @throws InterruptedException
	 */
	public void shutdownAndAwaitTermination(ExecutorService Pool) {
		if (alreadyShutdown) {
			alreadyShutdown = false;

			this.running = false;

			this.pool.shutdown(); // Disable new tasks from being submitted

			try {
				// Wait a while for existing tasks to terminate
				if (!this.pool.awaitTermination(10, TimeUnit.SECONDS)) {
					this.pool.shutdownNow(); // Cancel currently executing tasks
					// Wait a while for tasks to respond to being cancelled
					if (!this.pool.awaitTermination(10, TimeUnit.SECONDS)) {
						System.err.println("Pool did not terminate");
					}
				}
				// else {
				// this.pool.shutdownNow();
				// Thread.currentThread().interrupt();
				// }
			} catch (InterruptedException ie) {
				// (Re-)Cancel if current thread also interrupted
				this.pool.shutdownNow();
				// Preserve interrupt status
				Thread.currentThread().interrupt();
			}
		}
	}

	public void shutdown() throws IOException {
		synchronized (this.handlerList) {
			for (RpcHandler x : this.handlerList) {
				if (!x.getSocket().isClosed())
					x.exit();
			}
		}

		if (pool != null) {
			shutdownAndAwaitTermination(pool);
		}

		this.serverSocket.close();

		// this.interrupt();
	}

	public void closeConnection() throws IOException {
		synchronized (this.handlerList) {
			for (RpcHandler x : this.handlerList) {
				x.closeConnection();
			}
			this.handlerList.clear();
		}

		if (pool != null) {
			shutdownAndAwaitTermination(pool);
		}

		this.serverSocket.close();
	}

	public void reconnect() {
		try {
			this.serverSocket = new ServerSocket(this.port);
		} catch (IOException e) {
			logger.error("Error while reconnecting!");
		}
	}
}