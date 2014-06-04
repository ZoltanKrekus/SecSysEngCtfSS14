#include "SOCApplication.h"
#include "MenuWidget.h"

#include <Wt/Auth/AuthWidget>
#include <Wt/WContainerWidget>
#include <Wt/WEnvironment>
#include <Wt/WText>
#include <Wt/WImage>
#include <Wt/WMessageBox>

#include <Wt/WPushButton>

class PageWidget: public Wt::WTemplate {
	public:
		PageWidget(std::string str) {
			setTemplateText(tr("soc." + str));
		}
		virtual ~PageWidget() {
		}
};

template <typename WidgetType> Wt::WWidget* SOCApplication::_createPage(Wt::WWidget* w, std::string str) {
	if (!w) {
		w = new WidgetType(str);
		_content->addWidget(w);
	}
	w->doJavaScript("setIframes()");
	return w;
}

SOCApplication::SOCApplication(const Wt::WEnvironment& env, const std::string pwFileName) : Wt::WApplication(env), _session { pwFileName } {
	useStyleSheet("soc.css");
	messageResourceBundle().use("www/wt");
	messageResourceBundle().use("www/soc");
	_authWidget = new Wt::Auth::AuthWidget(Session::auth(), _session.users(), _session.login());
	_authWidget->model()->addPasswordAuth(&Session::passwordAuth());
	_authWidget->setRegistrationEnabled(false);

	_authWidget->processEnvironment();
	_session.login().changed().connect(this, &SOCApplication::onAuthEvent);
	internalPathChanged().connect(this, &SOCApplication::onInternalPathChange);

	setTitle("LCARS - Linux Can Also Run Starships");                               // application title
	_root_template = new Wt::WTemplate(Wt::WString::tr("soc.skeleton"), root());

	_menuWidget = new MenuWidget(_session);
	_content_template = new Wt::WTemplate(Wt::WString::tr("soc.page-content"));

	_root_template->bindWidget("page-content", _content_template);

	_content = new Wt::WStackedWidget();
	_content_template->bindWidget("page-content", _content);
	_content->addWidget(_authWidget);

	if (Wt::WApplication::instance()->environment().javaScript()) {
		Wt::WApplication::instance()->require("script.js");
	}

	setInternalPath("/login");
}

void SOCApplication::onAuthEvent() {
	if (_session.login().loggedIn()) {
		_root_template->bindWidget("page-navigation", _menuWidget);
		setInternalPath("/welcome");
		show(Page::Welcome);
		log("info") << "login successful";
	} else {
		setInternalPath("/login");
		show(Page::Login);
		log("info") << "access permitted";
	}
}

Wt::WWidget* SOCApplication::getPageElement(Page page) {
	switch (page) {
		case Page::Login:
			return _authWidget;

		case Page::Welcome:
			return _createPage<PageWidget>(_welcome, "welcome");

		case Page::HolodeckProgramDatabase:
			setTitle(_title + " - Holodeck-ProgDB"); // application title
			return _createPage<PageWidget>(_holodeckProgramDatabase, "holodeck");

		case Page::KSAP:
			setTitle(_title + " - K-SAP"); // application title
			return _createPage<PageWidget>(_ksap, "ksap");

		case Page::LogbookGenerator:
			setTitle(_title + " - Logbook Generator"); // application title
			return _createPage<PageWidget>(_logbookGenerator, "logbook");

		case Page::PlanetDatabase:
			setTitle(_title + " - Planet Database"); // application title
			return _createPage<PageWidget>(_planetDB, "planetDB");

		case Page::SpaceshipAI:
			setTitle(_title + " - Spaceship AI"); // application title
			return _createPage<PageWidget>(_spaceshipAI, "spaceshipAI");

		case Page::SpaceComService:
			setTitle(_title + " - SpaceComService"); // application title
			return _createPage<PageWidget>(_spacecom, "spacecomservice");

		case Page::Checklist:
			setTitle(_title + " - Checklist"); // application title
			return _createPage<PageWidget>(_checklist, "checklist");

		case Page::Impressum:
			setTitle(_title + " - Impressum"); // application title
			return _createPage<PageWidget>(_impressum, "impressum");
	}
	return nullptr;
}

void SOCApplication::onInternalPathChange() {
	if (internalPath() == "/login") {
		show(Page::Login);
		return;
	}

	if (!_session.login().loggedIn()) {
		setInternalPath("/login");
		return;
	}

	if (internalPath() == "/logout") {
		_session.login().logout();
		Wt::WApplication::instance()->redirect("/login");
		return;
	}

	if (internalPath() == "/welcome") {
		show(Page::Welcome);
		return;
	}

	if (internalPath() == "/holodeck") {
		show(Page::HolodeckProgramDatabase);
		return;
	}

	if (internalPath() == "/ksap") {
		show(Page::KSAP);
		return;
	}

	if (internalPath() == "/logbook") {
		show(Page::LogbookGenerator);
		return;
	}

	if (internalPath() == "/planetDB") {
		show(Page::PlanetDatabase);
		return;
	}

	if (internalPath() == "/spaceshipAI") {
		show(Page::SpaceshipAI);
		return;
	}

	if (internalPath() == "/spacecomservice") {
		show(Page::SpaceComService);
		return;
	}

	if (internalPath() == "/checklist") {
		show(Page::Checklist);
		return;
	}

	if (internalPath() == "/impressum") {
		show(Page::Impressum);
		return;
	}
}

void SOCApplication::show(Page page) {
	if (_current_page == page) {
		return;
	}

	_last_page = internalPath();
	_content->setCurrentWidget(getPageElement(page));

	_current_page = page;
}

