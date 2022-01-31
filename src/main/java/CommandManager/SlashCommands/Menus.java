package CommandManager.SlashCommands;

import CommandManager.ISlashCommand;
import RoleMenus.RWHSlashMenus;
import RoleMenus.SlashMenus;
import net.dv8tion.jda.api.Permission;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.events.interaction.ButtonClickEvent;
import net.dv8tion.jda.api.events.interaction.SlashCommandEvent;

import java.util.Arrays;
import java.util.List;

public class Menus implements ISlashCommand {

    @Override
    public void run(SlashCommandEvent event) throws Exception {
        if (event.getMember().hasPermission(Permission.MANAGE_ROLES)) {
            if (event.getSubcommandName().equals("yearroles")) SlashMenus.SlashYearMenu(event);
            if (event.getSubcommandName().equals("pronounroles")) SlashMenus.PronounMenu(event);
            if (event.getSubcommandName().equals("collegeroles")) SlashMenus.CollegeMenu(event);
            if (event.getSubcommandName().equals("polirole")) SlashMenus.PoliMenu(event);
            if (event.getSubcommandName().equals("concentration")) RWHSlashMenus.SlashConcetrationMenu(event);
            if (event.getSubcommandName().equals("ccievents")) RWHSlashMenus.SlashCCIEvents(event);
        } else {
            event.reply("You don't have the `MANAGE_ROLES` permission, which is required to run this command!").setEphemeral(true).queue();
        }
    }

    @Override
    public void run(ButtonClickEvent event) throws Exception {

        //Give or take Debate role
        if (event.getComponentId().startsWith("POLI_")) {
            final long DEBATE = event.getGuild().getRolesByName("Debate", true).get(0).getIdLong();
            switch (event.getComponentId()) {
                case ("POLI_YES"):
                    if (!event.getGuild().getMembersWithRoles(event.getGuild().getRoleById(DEBATE)).contains(event.getMember())) {
                        event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(DEBATE)).queue();
                        event.reply("You will now have access to politics!").setEphemeral(true).queue();
                    } else {
                        event.reply("You have already opted in for politics!").setEphemeral(true).queue();
                    }
                    break;
                case ("POLI_NO"):
                    if (event.getGuild().getMembersWithRoles(event.getGuild().getRoleById(DEBATE)).contains(event.getMember())) {
                        event.getGuild().removeRoleFromMember(event.getMember(), event.getGuild().getRoleById(DEBATE)).queue();
                        event.reply("You will no longer have access to politics!").setEphemeral(true).queue();
                    } else {
                        event.reply("You don't need to opt-out for politics if you never opted-in!").setEphemeral(true).queue();
                    }
                    break;
            }
        }

        //Give or take CCI Events role
        if (event.getComponentId().startsWith("CCI_")) {

            Guild guild = event.getGuild();
            long CCI_EVENTS = guild.getRolesByName("CCI Events", true).get(0).getIdLong();

            switch (event.getComponentId()) {
                case ("CCI_YES_CCIEVENTS"):
                    if (!event.getGuild().getMembersWithRoles(event.getGuild().getRoleById(CCI_EVENTS)).contains(event.getMember())) {
                        event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(CCI_EVENTS)).queue();
                        event.reply("You will now receive pings for events!").setEphemeral(true).queue();
                    } else {
                        event.reply("You have already opted in for pings!").setEphemeral(true).queue();
                    }
                    break;
                case ("CCI_NO_CCIEVENTS"):
                    if (event.getGuild().getMembersWithRoles(event.getGuild().getRoleById(CCI_EVENTS)).contains(event.getMember())) {
                        event.getGuild().removeRoleFromMember(event.getMember(), event.getGuild().getRoleById(CCI_EVENTS)).queue();
                        event.reply("You will no longer receive pings for events!").setEphemeral(true).queue();
                    } else {
                        event.reply("You don't need to opt-out for pings if you never opted-in!").setEphemeral(true).queue();
                    }
                    break;
            }
        }

        if (event.getComponentId().startsWith("College_")) {
            final long DATA = event.getGuild().getRolesByName("Data Science", true).get(0).getIdLong();
            final long LIBERAL = event.getGuild().getRolesByName("Liberal Arts & Sciences", true).get(0).getIdLong();
            final long HEALTH = event.getGuild().getRolesByName("Health & Human Services", true).get(0).getIdLong();
            final long ENGINEER = event.getGuild().getRolesByName("Engineering", true).get(0).getIdLong();
            final long EDU = event.getGuild().getRolesByName("Education", true).get(0).getIdLong();
            final long COMPUTER = event.getGuild().getRolesByName("Computing & Informatics", true).get(0).getIdLong();
            final long ARTS = event.getGuild().getRolesByName("Arts + Architecture", true).get(0).getIdLong();
            final long BUSINESS = event.getGuild().getRolesByName("Business", true).get(0).getIdLong();
            final long UNDEC = event.getGuild().getRolesByName("Undeclared", true).get(0).getIdLong();

            // Remove college role
            SlashMenus.RemoveCollegeRole(event);

            switch (event.getComponentId()) {
                case ("College_Data"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(DATA)).queue();
                    event.reply("Added Data Science role!").setEphemeral(true).queue();
                    break;
                case ("College_Liberal"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(LIBERAL)).queue();
                    event.reply("Added Liberal Arts role!").setEphemeral(true).queue();
                    break;
                case ("College_Health"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(HEALTH)).queue();
                    event.reply("Added Health role!").setEphemeral(true).queue();
                    break;
                case ("College_Engineering"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(ENGINEER)).queue();
                    event.reply("Added Engineering role!").setEphemeral(true).queue();
                    break;
                case ("College_Education"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(EDU)).queue();
                    event.reply("Added Education role!").setEphemeral(true).queue();
                    break;
                case ("College_Computing"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(COMPUTER)).queue();
                    event.reply("Added CCI role!").setEphemeral(true).queue();
                    break;
                case ("College_Arts"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(ARTS)).queue();
                    event.reply("Added Arts + Architecture role!").setEphemeral(true).queue();
                    break;
                case ("College_Business"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(BUSINESS)).queue();
                    event.reply("Added Business role!").setEphemeral(true).queue();
                    break;
                case ("College_Undec"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(UNDEC)).queue();
                    event.reply("Added Undeclared role!").setEphemeral(true).queue();
                    break;
            }
        }

        // Give year role to memeber
        if (event.getComponentId().startsWith("Pron_")) {

            final long HE = event.getGuild().getRolesByName("He/Him", true).get(0).getIdLong();
            final long SHE = event.getGuild().getRolesByName("She/Her", true).get(0).getIdLong();
            final long THEY = event.getGuild().getRolesByName("They/Them", true).get(0).getIdLong();
            final long HETHEY = event.getGuild().getRolesByName("He/They", true).get(0).getIdLong();
            final long SHETHEY = event.getGuild().getRolesByName("She/They", true).get(0).getIdLong();
            final long ASK = event.getGuild().getRolesByName("Ask pronouns", true).get(0).getIdLong();

            // Remove existing pronoun role
            SlashMenus.RemovePronRole(event);
            switch (event.getComponentId()) {
                case ("Pron_He"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(HE)).queue();
                    event.reply("Added He/Him role!").setEphemeral(true).queue();
                    break;
                case ("Pron_She"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(SHE)).queue();
                    event.reply("Added She/Her role!").setEphemeral(true).queue();
                    break;
                case ("Pron_They"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(THEY)).queue();
                    event.reply("Added They/Them role!").setEphemeral(true).queue();
                    break;
                case ("Pron_HeThey"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(HETHEY)).queue();
                    event.reply("Added He/They role!").setEphemeral(true).queue();
                    break;
                case ("Pron_SheThey"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(SHETHEY)).queue();
                    event.reply("Added She/They role!").setEphemeral(true).queue();
                    break;
                case ("Pron_Ask"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(ASK)).queue();
                    event.reply("Added Ask Me role!").setEphemeral(true).queue();
                    break;
            }
        }

        // Give year role to memeber
        if (event.getComponentId().startsWith("Year_")) {

            final long INCOMING = event.getGuild().getRolesByName("Incoming student", true).get(0).getIdLong();
            final long FRESHMAN = event.getGuild().getRolesByName("Freshman", true).get(0).getIdLong();
            final long SOPHOMORE = event.getGuild().getRolesByName("Sophomore", true).get(0).getIdLong();
            final long JUNIOR = event.getGuild().getRolesByName("Junior", true).get(0).getIdLong();
            final long SENIOR = event.getGuild().getRolesByName("Senior", true).get(0).getIdLong();
            final long GRADUATE = event.getGuild().getRolesByName("Graduate Student", true).get(0).getIdLong();
            final long ALUMNI = event.getGuild().getRolesByName("Alumni", true).get(0).getIdLong();

            // Remove existing year role
            SlashMenus.RemoveYearRole(event);
            switch (event.getComponentId()) {
                case ("Year_Incoming"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(INCOMING)).queue();
                    event.reply("Added Incoming Student role!").setEphemeral(true).queue();
                    break;
                case ("Year_Freshman"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(FRESHMAN)).queue();
                    event.reply("Added Freshman role!").setEphemeral(true).queue();
                    break;
                case ("Year_Sophomore"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(SOPHOMORE)).queue();
                    event.reply("Added Sophomore role!").setEphemeral(true).queue();
                    break;
                case ("Year_Junior"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(JUNIOR)).queue();
                    event.reply("Added Junior role!").setEphemeral(true).queue();
                    break;
                case ("Year_Senior"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(SENIOR)).queue();
                    event.reply("Added Senior role!").setEphemeral(true).queue();
                    break;
                case ("Year_Grad"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(GRADUATE)).queue();
                    event.reply("Added Grad student role!").setEphemeral(true).queue();
                    break;
                case ("Year_Alumni"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(ALUMNI)).queue();
                    event.reply("Added Alumni role!").setEphemeral(true).queue();
                    break;
            }
        }
        // Give concentration role to memeber
        if (event.getComponentId().startsWith("Conc_")) {

            Guild guild = event.getGuild();

            long AI_GAMING = guild.getRolesByName("ai-gaming", true).get(0).getIdLong();
            long DATA_SCIENCE = guild.getRolesByName("data-sci", true).get(0).getIdLong();
            long SOFTWARE_SYSTEMS = guild.getRolesByName("software-systems", true).get(0).getIdLong();
            long CYBER_SECURITY = guild.getRolesByName("cyber-sec", true).get(0).getIdLong();
            long HCI = guild.getRolesByName("hci", true).get(0).getIdLong();
            long INFO_TECH = guild.getRolesByName("info-tech", true).get(0).getIdLong();
            long SOFTWARE_ENGINEER = guild.getRolesByName("software-eng", true).get(0).getIdLong();
            long WEB_MOBILE = guild.getRolesByName("web-mobile", true).get(0).getIdLong();
            long BIO_INFORMATICS = guild.getRolesByName("bio-inf", true).get(0).getIdLong();
            long UNDECLARED = guild.getRolesByName("Undeclared", true).get(0).getIdLong();

            //remove existing concentration role
            RWHSlashMenus.RemoveConcRole(event);
            switch (event.getComponentId()) {
                case ("Conc_SE"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(SOFTWARE_ENGINEER)).queue();
                    event.reply("Added Software Engineering role!").setEphemeral(true).queue();
                    break;
                case ("Conc_Bioinformatics"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(BIO_INFORMATICS)).queue();
                    event.reply("Added Bioinformatics role!").setEphemeral(true).queue();
                    break;
                case ("Conc_ARG"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(AI_GAMING)).queue();
                    event.reply("Added AI, Robotics, and Gaming role!").setEphemeral(true).queue();
                    break;
                case ("Conc_Data Science"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(DATA_SCIENCE)).queue();
                    event.reply("Added Data Science role!").setEphemeral(true).queue();
                    break;
                case ("Conc_IT"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(INFO_TECH)).queue();
                    event.reply("Added Information Technology role!").setEphemeral(true).queue();
                    break;
                case ("Conc_WM"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(WEB_MOBILE)).queue();
                    event.reply("Added Web and Mobile role!").setEphemeral(true).queue();
                    break;
                case ("Conc_HCI"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(HCI)).queue();
                    event.reply("Added Human-Computer Interaction role!").setEphemeral(true).queue();
                    break;
                case ("Conc_Cybersecurity"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(CYBER_SECURITY)).queue();
                    event.reply("Added Cybersecurity role!").setEphemeral(true).queue();
                    break;
                case ("Conc_SSN"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(SOFTWARE_SYSTEMS)).queue();
                    event.reply("Added Software, Systems, and Networks role!").setEphemeral(true).queue();
                    break;
                case ("Conc_UD"):
                    event.getGuild().addRoleToMember(event.getMember(), event.getGuild().getRoleById(UNDECLARED)).queue();
                    event.reply("Added Undeclared role!").setEphemeral(true).queue();
                    break;
            }
        }
    }

    @Override
    public List<String> buttons() {
        return Arrays.asList("Year_Incoming", "Year_Freshman", "Year_Sophomore", "Year_Junior", "Year_Senior", "Year_Grad", "Year_Alumni", "Conc_SE", "Conc_Bioinformatics",
                "Conc_ARG", "Conc_Data Science", "Conc_IT", "Conc_WM", "Conc_HCI", "Conc_Cybersecurity", "Conc_SSN", "Conc_UD", "CCI_YES_CCIEVENTS", "CCI_NO_CCIEVENTS",
                "Pron_He", "Pron_She", "Pron_They", "Pron_HeThey", "Pron_SheThey", "Pron_Ask", "College_Data", "College_Liberal", "College_Health",
                "College_Engineering", "College_Education", "College_Computing", "College_Arts", "College_Business", "College_Undec", "POLI_YES", "POLI_NO");
    }

    @Override
    public String commandName() {
        return "menus";
    }

    @Override
    public Boolean enabled() {
        return true;
    }

    @Override
    public String description() {
        return null;
    }
}