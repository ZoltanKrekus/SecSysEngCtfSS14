#ifndef _SPACESHIP_APPLICATION_H
#define _SPACESHIP_APPLICATION_H

#include <Wt/WApplication>
#include <Wt/WStackedWidget>
#include <Wt/WTemplate>

#include <Wt/WText>
#include <Wt/WLineEdit>

#include "Session.h"

class SOCApplication : public Wt::WApplication {
	public:
		void onInternalPathChange();
		void onAuthEvent();

		SOCApplication(const Wt::WEnvironment& env, const std::string pwFileName);

	private:
		Wt::WLineEdit *nameEdit_;
		Wt::WText *greeting_;

		enum class Page {
			Login,
			Welcome,
			HolodeckProgramDatabase,
			KSAP,
			LogbookGenerator,
			PlanetDatabase,
			SpaceshipAI,
			SpaceComService,
			Checklist,
			Impressum,
		};

		std::string _last_page = "/";
		std::string _title = "LCARS - Linux Can Also Run Starships";

		void show(Page page);
		Wt::WWidget* getPageElement(Page page);

		Session _session;
		Page _current_page = Page::Login;

		Wt::Auth::AuthWidget *_authWidget;

		Wt::WTemplate* _root_template;
		Wt::WTemplate* _content_template;
		Wt::WStackedWidget* _content;

		Wt::WWidget* _menuWidget = nullptr;
		Wt::WWidget* _welcome = nullptr;
		Wt::WWidget* _holodeckProgramDatabase = nullptr;
		Wt::WWidget* _ksap = nullptr;
		Wt::WWidget* _logbookGenerator = nullptr;
		Wt::WWidget* _planetDB = nullptr;
		Wt::WWidget* _spaceshipAI = nullptr;
		Wt::WWidget* _spacecom = nullptr;
		Wt::WWidget* _checklist = nullptr;
		Wt::WWidget* _impressum = nullptr;

		template <typename WidgetType> Wt::WWidget* _createPage(Wt::WWidget* w, std::string str);
};


#endif //_SPACESHIP_APPLICATION_H
