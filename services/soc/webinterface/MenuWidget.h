#ifndef __MENUWIDGET_H
#define __MENUWIDGET_H

#include <Wt/WTemplate>

#include "Session.h"

class MenuWidget : public Wt::WTemplate {
	public:
		MenuWidget(Session& session);

		Wt::WWidget* createLink(const std::string& path, const std::string& description);
};

#endif
