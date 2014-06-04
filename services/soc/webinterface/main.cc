#include "SOCApplication.h"

#include <Wt/WServer>
//#include <Wt/WApplication>
//#include <Wt/WBreak>
//#include <Wt/WContainerWidget>
//#include <Wt/WLineEdit>
//#include <Wt/WPushButton>
//#include <Wt/WText>

std::string pwFileName = "soc.passwd";

int main(int argc, char **argv) {
	Wt::WServer server(argv[0]);
	std::cout << "setting server configuration" << std::endl;
	server.setServerConfiguration(argc, argv, WTHTTP_CONFIGURATION);
	server.addEntryPoint(Wt::Application, [](const Wt::WEnvironment& env) -> Wt::WApplication* { return new SOCApplication(env, pwFileName); });

	Session::configureAuth();
	if (server.start()) {
		Wt::WServer::waitForShutdown();
		server.stop();
	}
	return 0;
}

