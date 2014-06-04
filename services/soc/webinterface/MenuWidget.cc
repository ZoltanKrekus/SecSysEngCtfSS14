#include "MenuWidget.h"

#include <Wt/WLink>
#include <Wt/WAnchor>

MenuWidget::MenuWidget(Session& session) {
	setTemplateText(tr("soc.navigation"));

	bindWidget("nav-welcome", createLink("/welcome", "Welcome"));
	bindWidget("nav-holodeck", createLink("/holodeck", "Holodeck-ProgDB"));
	bindWidget("nav-ksap", createLink("/ksap", "K-SAP"));
	bindWidget("nav-logbook", createLink("/logbook", "Logbook Generator"));
	bindWidget("nav-planetDB", createLink("/planetDB", "Planet Database"));
	bindWidget("nav-spaceshipAI", createLink("/spaceshipAI", "Spaceship AI"));
	bindWidget("nav-spacecomservice", createLink("/spacecomservice", "Spacecom Service"));
	bindWidget("nav-checklist", createLink("/checklist", "Checklist"));
	bindWidget("nav-impressum", createLink("/impressum", "Impressum"));
	bindWidget("nav-logout", createLink("/logout", "Logout"));
}

Wt::WWidget* MenuWidget::createLink(const std::string& path, const std::string& description) {
	auto element = new WTemplate(tr("soc.navigation-element"));
	element->bindWidget("navigation-element", new Wt::WAnchor(Wt::WLink(Wt::WLink::InternalPath, path), description));
	return element;
}

