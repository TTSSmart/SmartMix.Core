using FluentMigrator;

namespace SmartMix.Core.Infrastructure.Persistence.Migrations;

[Migration(20240101001)]
public class InitialSchema : Migration
{
    public override void Up()
    {
        // Users
        Create.Table("users")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("username").AsString(50).NotNullable().Unique()
            .WithColumn("password_hash").AsString(255).NotNullable()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("role").AsString(50).NotNullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime);
        
        // Applications
        Create.Table("applications")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("waybill").AsString(50).Nullable()
            .WithColumn("client_id").AsInt32().NotNullable()
            .WithColumn("car_id").AsInt32().NotNullable()
            .WithColumn("volume").AsDecimal(10, 3).NotNullable()
            .WithColumn("fact_volume").AsDecimal(10, 3).Nullable()
            .WithColumn("volume_current").AsDecimal(10, 3).NotNullable()
            .WithColumn("creation_time").AsDateTime().NotNullable()
            .WithColumn("start_time").AsDateTime().Nullable()
            .WithColumn("end_time").AsDateTime().Nullable()
            .WithColumn("is_completed").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("creator_id").AsInt32().NotNullable()
            .WithColumn("user_id").AsInt32().NotNullable()
            .WithColumn("mixer_number").AsInt32().NotNullable()
            .WithColumn("order_num").AsInt32().NotNullable()
            .WithColumn("batch_phase").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("train_mode").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("is_speed_app").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("last_save_batch_num").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("is_running").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("is_edit_lock").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("product_id").AsInt32().Nullable()
            .WithColumn("unloading_point").AsInt32().Nullable()
            .WithColumn("correct_water_panel").AsDecimal(10, 3).Nullable()
            .WithColumn("start_auto").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("line_number").AsInt32().NotNullable().WithDefaultValue(1);
        
        // Application Layers
        Create.Table("application_layers")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("application_id").AsInt32().NotNullable()
            .WithColumn("number").AsInt32().NotNullable()
            .WithColumn("recipe_id").AsInt32().Nullable()
            .WithColumn("volume").AsDecimal(10, 3).NotNullable()
            .WithColumn("cur_volume").AsDecimal(10, 3).Nullable()
            .WithColumn("fact_volume").AsDecimal(10, 3).Nullable()
            .WithColumn("status").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("batcher_performed").AsInt32().NotNullable().WithDefaultValue(0);
        
        Create.ForeignKey("fk_application_layers_application")
            .FromTable("application_layers").ForeignColumn("application_id")
            .ToTable("applications").PrimaryColumn("id");
        
        // Recipes
        Create.Table("recipes")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("consider").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("use_auto_correction").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("user_id").AsInt32().NotNullable()
            .WithColumn("edit_date").AsDateTime().NotNullable()
            .WithColumn("time_set_id").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("category_id").AsInt32().Nullable()
            .WithColumn("mixer_set_id").AsInt32().NotNullable().WithDefaultValue(1);
        
        // Recipe Structures
        Create.Table("recipe_structures")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("recipe_id").AsInt32().NotNullable()
            .WithColumn("component_id").AsInt32().NotNullable()
            .WithColumn("component_name").AsString(100).NotNullable()
            .WithColumn("percentage").AsDecimal(10, 3).NotNullable()
            .WithColumn("target_weight").AsDecimal(10, 3).NotNullable()
            .WithColumn("correct").AsDecimal(10, 3).NotNullable().WithDefaultValue(0);
        
        Create.ForeignKey("fk_recipe_structures_recipe")
            .FromTable("recipe_structures").ForeignColumn("recipe_id")
            .ToTable("recipes").PrimaryColumn("id");
        
        // Recipe Categories
        Create.Table("recipe_categories")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("description").AsString(255).Nullable();
        
        // Recipe Time Sets
        Create.Table("recipe_time_sets")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("mix_time").AsInt32().NotNullable().WithDefaultValue(60)
            .WithColumn("unload_time").AsInt32().NotNullable().WithDefaultValue(10)
            .WithColumn("extra_unload_time").AsInt32().NotNullable().WithDefaultValue(3);
        
        // Recipe Mixer Sets
        Create.Table("recipe_mixer_sets")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("name").AsString(100).NotNullable();
        
        // Recipe Mixer Set Items
        Create.Table("recipe_mixer_set_items")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("mixer_set_id").AsInt32().NotNullable()
            .WithColumn("gate_number").AsInt32().NotNullable()
            .WithColumn("pulse_count").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("pulse_time").AsInt32().NotNullable().WithDefaultValue(500)
            .WithColumn("delay_time").AsInt32().NotNullable().WithDefaultValue(200)
            .WithColumn("dont_close_after_impulse").AsBoolean().NotNullable().WithDefaultValue(false);
        
        // Components
        Create.Table("components")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("type").AsInt32().NotNullable()
            .WithColumn("density").AsDecimal(10, 3).Nullable()
            .WithColumn("moisture").AsDecimal(5, 2).Nullable();
        
        // Bunkers
        Create.Table("bunkers")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("number").AsInt32().NotNullable()
            .WithColumn("line_number").AsInt32().NotNullable()
            .WithColumn("is_on").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("component_id").AsInt32().Nullable()
            .WithColumn("title").AsString(100).Nullable();
        
        Create.Index("ix_bunkers_line_number").OnTable("bunkers").OnColumn("line_number");
        Create.Index("ux_bunkers_line_number_number").OnTable("bunkers").OnColumn("line_number").OnColumn("number").Unique();
        
        // Clients
        Create.Table("clients")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("code").AsString(50).Nullable()
            .WithColumn("inn").AsString(20).Nullable()
            .WithColumn("address").AsString(255).Nullable()
            .WithColumn("phone").AsString(50).Nullable();
        
        // Cars
        Create.Table("cars")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("number").AsString(20).NotNullable()
            .WithColumn("model").AsString(100).Nullable()
            .WithColumn("client_id").AsInt32().Nullable()
            .WithColumn("tara_weight").AsDecimal(10, 3).Nullable()
            .WithColumn("max_volume").AsDecimal(10, 3).Nullable();
        
        // Products
        Create.Table("products")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("code").AsString(50).Nullable()
            .WithColumn("description").AsString(255).Nullable();
        
        // Moto Hours
        Create.Table("moto_hours")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("mechanism_type").AsInt32().NotNullable()
            .WithColumn("mechanism_number").AsInt32().NotNullable()
            .WithColumn("hours").AsDecimal(10, 2).NotNullable()
            .WithColumn("recorded_at").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime);
        
        Create.Index("ix_moto_hours_type_number").OnTable("moto_hours").OnColumn("mechanism_type").OnColumn("mechanism_number");
        
        // Settings
        Create.Table("settings")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("setting_key").AsString(100).NotNullable()
            .WithColumn("setting_value").AsString(4000).Nullable()
            .WithColumn("line_number").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("updated_at").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime);
        
        Create.Index("ux_settings_key_line").OnTable("settings").OnColumn("setting_key").OnColumn("line_number").Unique();
        
        // Consumption Log
        Create.Table("consumption_log")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("bunker_number").AsInt32().NotNullable()
            .WithColumn("material_id").AsInt32().NotNullable()
            .WithColumn("weight_kg").AsDecimal(10, 3).NotNullable()
            .WithColumn("application_id").AsInt32().Nullable()
            .WithColumn("batch_number").AsInt32().Nullable()
            .WithColumn("recorded_at").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime);
        
        Create.Index("ix_consumption_log_bunker_time").OnTable("consumption_log").OnColumn("bunker_number").OnColumn("recorded_at");
        Create.Index("ix_consumption_log_application").OnTable("consumption_log").OnColumn("application_id");
        
        // Events Log
        Create.Table("events_log")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("event_type").AsInt32().NotNullable()
            .WithColumn("event_level").AsInt32().NotNullable()
            .WithColumn("mechanism_type").AsInt32().Nullable()
            .WithColumn("mechanism_number").AsInt32().Nullable()
            .WithColumn("message").AsString(500).NotNullable()
            .WithColumn("details").AsString(2000).Nullable()
            .WithColumn("application_id").AsInt32().Nullable()
            .WithColumn("user_id").AsInt32().Nullable()
            .WithColumn("recorded_at").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime);
        
        Create.Index("ix_events_log_time").OnTable("events_log").OnColumn("recorded_at");
        Create.Index("ix_events_log_application").OnTable("events_log").OnColumn("application_id");
        
        // Alarms Log
        Create.Table("alarms_log")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("mechanism_type").AsInt32().NotNullable()
            .WithColumn("mechanism_number").AsInt32().NotNullable()
            .WithColumn("alarm_code").AsInt32().NotNullable()
            .WithColumn("alarm_level").AsInt32().NotNullable()
            .WithColumn("message").AsString(500).NotNullable()
            .WithColumn("raised_at").AsDateTime().NotNullable()
            .WithColumn("cleared_at").AsDateTime().Nullable()
            .WithColumn("acknowledged_by").AsInt32().Nullable()
            .WithColumn("acknowledged_at").AsDateTime().Nullable();
        
        Create.Index("ix_alarms_log_mechanism").OnTable("alarms_log").OnColumn("mechanism_type").OnColumn("mechanism_number");
        Create.Index("ix_alarms_log_raised").OnTable("alarms_log").OnColumn("raised_at");
        
        // License Info
        Create.Table("license_info")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("serial_number").AsString(100).NotNullable()
            .WithColumn("hardware_id").AsString(100).NotNullable()
            .WithColumn("license_type").AsInt32().NotNullable()
            .WithColumn("end_date").AsDateTime().NotNullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("activated_at").AsDateTime().Nullable()
            .WithColumn("max_lines").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("max_mixers").AsInt32().NotNullable().WithDefaultValue(1);
    }
    
    public override void Down()
    {
        Delete.Table("license_info");
        Delete.Table("alarms_log");
        Delete.Table("events_log");
        Delete.Table("consumption_log");
        Delete.Table("settings");
        Delete.Table("moto_hours");
        Delete.Table("products");
        Delete.Table("cars");
        Delete.Table("clients");
        Delete.Table("bunkers");
        Delete.Table("components");
        Delete.Table("recipe_mixer_set_items");
        Delete.Table("recipe_mixer_sets");
        Delete.Table("recipe_time_sets");
        Delete.Table("recipe_categories");
        Delete.Table("recipe_structures");
        Delete.Table("recipes");
        Delete.Table("application_layers");
        Delete.Table("applications");
        Delete.Table("users");
    }
}